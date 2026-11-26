# 以 GitHub Copilot CLI 執行委託 AI Tools Review（見 .github/team/tool-copilot-cli.md）
# 用法：在工作目錄（主 checkout 或 worktree）執行
#   powershell -ExecutionPolicy Bypass -File .github\scripts\Invoke-CopilotReview.ps1 -ListModels
#   powershell -ExecutionPolicy Bypass -File .github\scripts\Invoke-CopilotReview.ps1 -Model <模型> -Effort <推理強度>
# 有失敗時 exit code 非 0

param(
	[switch]$ListModels,
	[string]$Model,
	[string]$Effort,
	[string]$CopilotPath
)

$ErrorActionPreference = 'Stop'
$REQUEST_FILE = 'REVIEW-REQUEST.md'
$STATUS_PENDING = '狀態：待 review'
$STATUS_DONE = '狀態：完成'
$REVIEW_PROMPT = '請讀取工作目錄根目錄的 REVIEW-REQUEST.md，並完全依照其中的對象、範圍與 Reviewer 規則執行 review。'
$ACP_TIMEOUT_MS = 60000

# 依序找參數、COPILOT_CLI_PATH、PATH、預設安裝位置
function Find-Copilot()
{
	$candidateList = @( $CopilotPath, $env:COPILOT_CLI_PATH )
	$cmd = Get-Command 'copilot.exe' -ErrorAction SilentlyContinue
	if( $null -ne $cmd ) {
		$candidateList += $cmd.Source
	}
	$candidateList += Join-Path $env:LOCALAPPDATA 'Programs\CopilotCli\copilot.exe'
	foreach( $szPath in $candidateList ) {
		if( -not [string]::IsNullOrEmpty( $szPath ) -and ( Test-Path -LiteralPath $szPath ) ) {
			return $szPath
		}
	}
	throw '找不到 copilot.exe，安裝方式見 .github/team/env-ai-tools.md'
}

# 從 Copilot Agent（如 VS Copilot）內呼叫時，子程序會繼承 Agent 內建 CLI 的設定位置，找不到另裝 CLI 的登入資訊
function Clear-InheritedCopilotEnv()
{
	if( $env:COPILOT_CLI -ne '1' ) {
		return
	}
	$nameArray = @( 'COPILOT_HOME', 'XDG_CONFIG_HOME', 'XDG_STATE_HOME', 'COPILOT_CLI', 'COPILOT_AGENT_SESSION_ID',
		'COPILOT_CLI_BINARY_VERSION', 'COPILOT_CLI_RESOLVED_DIST_DIR', 'COPILOT_RUNTIME_PROCESS_FILE_LOGGING' )
	foreach( $szName in $nameArray ) {
		Remove-Item "Env:$szName" -ErrorAction SilentlyContinue
	}
}

# ACP（Agent Client Protocol）：stdin / stdout 逐行 JSON-RPC
function Invoke-AcpCall( $Process, [int]$Id, [string]$Method, $Params )
{
	$szRequest = @{ jsonrpc = '2.0'; id = $Id; method = $Method; params = $Params } | ConvertTo-Json -Depth 10 -Compress
	$byteArray = [Text.Encoding]::UTF8.GetBytes( $szRequest + "`n" )
	$Process.StandardInput.BaseStream.Write( $byteArray, 0, $byteArray.Length )
	$Process.StandardInput.BaseStream.Flush()
	while( $true ) {
		$task = $Process.StandardOutput.ReadLineAsync()
		if( -not $task.Wait( $ACP_TIMEOUT_MS ) ) {
			throw "ACP $Method 逾時"
		}

		# 程序已結束
		if( $null -eq $task.Result ) {
			throw "ACP $Method 沒有回應"
		}
		$msg = $task.Result | ConvertFrom-Json
		if( $msg.id -ne $Id ) {
			continue
		}
		if( $null -ne $msg.error ) {
			throw "ACP $Method 失敗：$( $msg.error.message )"
		}
		return $msg.result
	}
}

function Get-EffortText( $ConfigOptions )
{
	$effortOption = @( $ConfigOptions | Where-Object { $_.id -eq 'reasoning_effort' } ) | Select-Object -First 1
	if( $null -eq $effortOption ) {
		return '（不支援）'
	}
	$valueArray = @( $effortOption.options | ForEach-Object { $_.value } )
	return "$( $valueArray -join ', ' )（預設 $( $effortOption.currentValue )）"
}

function Show-ModelList( [string]$Exe )
{
	$startInfo = New-Object Diagnostics.ProcessStartInfo
	$startInfo.FileName = $Exe
	$startInfo.Arguments = '--acp'
	$startInfo.UseShellExecute = $false
	$startInfo.RedirectStandardInput = $true
	$startInfo.RedirectStandardOutput = $true
	$startInfo.RedirectStandardError = $true
	$startInfo.StandardOutputEncoding = [Text.Encoding]::UTF8
	$startInfo.WorkingDirectory = ( Get-Location ).Path

	# stdin 沿用主控台編碼；UTF-8 主控台會先送出 BOM，CLI 無法解析第一則訊息
	[Console]::InputEncoding = New-Object Text.UTF8Encoding $false
	$process = [Diagnostics.Process]::Start( $startInfo )

	# stderr 不讀會塞滿管線，讓 CLI 停住
	$process.BeginErrorReadLine()
	try {
		$null = Invoke-AcpCall $process 1 'initialize' @{ protocolVersion = 1; clientCapabilities = @{} }
		$session = Invoke-AcpCall $process 2 'session/new' @{ cwd = ( Get-Location ).Path; mcpServers = @() }

		# 推理強度的選項依模型而定，逐一切換模型讀取
		$rowList = @()
		$nId = 10
		foreach( $modelInfo in $session.models.availableModels ) {
			if( $modelInfo.modelId -eq 'auto' ) {
				continue
			}
			$nId++
			$setParams = @{ sessionId = $session.sessionId; configId = 'model'; value = $modelInfo.modelId }
			$result = Invoke-AcpCall $process $nId 'session/set_config_option' $setParams
			$rowList += [pscustomobject]@{
				Model = $modelInfo.modelId
				Cost = $modelInfo._meta.copilotPriceCategory
				Usage = $modelInfo._meta.copilotUsage
				Effort = Get-EffortText $result.configOptions
			}
		}
		$rowList | Format-Table -AutoSize | Out-String -Width 200 | Write-Output
	}
	finally {
		if( -not $process.HasExited ) {
			$process.Kill()
		}
	}
}

function Invoke-Review( [string]$Exe )
{
	# 請求檔不存在或不是待 review
	if( -not ( Test-Path -LiteralPath $REQUEST_FILE ) ) {
		throw "工作目錄沒有 $REQUEST_FILE，範本見 .github/team/process-ai-review.md 第 4 節"
	}
	$szStatus = ( Get-Content -LiteralPath $REQUEST_FILE -TotalCount 1 -Encoding UTF8 ).Trim()
	if( $szStatus -ne $STATUS_PENDING ) {
		throw "$REQUEST_FILE 第一行不是「$STATUS_PENDING」：$szStatus"
	}
	if( [string]::IsNullOrEmpty( $Model ) ) {
		throw '缺少 -Model；先以 -ListModels 列出可用模型，由人決定'
	}

	# worktree 的 git 資料在主 checkout 的 .git/，需加入讀取範圍
	$szGitDir = ( git rev-parse --path-format=absolute --git-common-dir ).Trim()
	$argList = @( '-p', $REVIEW_PROMPT, '--model', $Model, '--allow-all', '--no-ask-user', '-s', '--add-dir', $szGitDir )
	if( -not [string]::IsNullOrEmpty( $Effort ) ) {
		$argList += @( '--reasoning-effort', $Effort )
	}
	$szHeadBefore = ( git rev-parse HEAD ).Trim()
	$szBranchBefore = ( git branch --show-current ).Trim()
	& $Exe @argList
	$nExitCode = $LASTEXITCODE

	# reviewer 只能改報告檔與請求檔，不能 commit 或切換分支；其餘異動由施工方處理
	Write-Output ''
	Write-Output '=== 工作樹異動（應只有報告檔與請求檔）==='
	git status --short
	$isSameHead = ( ( git rev-parse HEAD ).Trim() -eq $szHeadBefore ) -and ( ( git branch --show-current ).Trim() -ceq $szBranchBefore )
	if( -not $isSameHead ) {
		Write-Output "=== HEAD 或分支已變動：執行前 $szBranchBefore $szHeadBefore ==="
	}
	$szStatus = ( Get-Content -LiteralPath $REQUEST_FILE -TotalCount 1 -Encoding UTF8 ).Trim()
	Write-Output "=== ${REQUEST_FILE}：$szStatus ==="
	if( $nExitCode -ne 0 ) {
		exit $nExitCode
	}

	# 只接受約定的完成狀態；「失敗：<原因>」的原因可能含「完成」二字
	if( ( $szStatus -ne $STATUS_DONE ) -or -not $isSameHead ) {
		exit 1
	}
}

$env:COPILOT_AUTO_UPDATE = 'false'
Clear-InheritedCopilotEnv
$szExe = Find-Copilot
if( $ListModels ) {
	Show-ModelList $szExe
}
else {
	Invoke-Review $szExe
}
