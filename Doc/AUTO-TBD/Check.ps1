# AUTO-TBD 議題限定檢查：執行 TestPlan.md 中標「自動」的 TEST
# 用法：在 repo 任一位置執行 powershell -ExecutionPolicy Bypass -File Doc\AUTO-TBD\Check.ps1 [-Base <commit>]
# 有失敗時 exit code 為失敗的 TEST 數

[CmdletBinding()]
param(
	[string]$Base = '24c7276d',

	# 個人設定只存在實作者的電腦，其他成員執行時略過 TEST-12
	[switch]$SkipPersonal
)

$ErrorActionPreference = 'Stop'
$Root = (git rev-parse --show-toplevel).Trim()
Set-Location $Root
$Results = New-Object System.Collections.Generic.List[object]

# 比對基準不存在時 git diff 只會回傳空清單，各 TEST 會誤判通過
git rev-parse --verify --quiet "$Base^{commit}" | Out-Null
if( $LASTEXITCODE -ne 0 ) {
	Write-Output "比對基準 $Base 不是有效的 commit"
	exit 100
}

# ============================================================
# 共用函式
# ============================================================

function Add-Result( [string]$Id, [string[]]$Failures, [string[]]$Notes = @() )
{
	$isPass = ( $Failures.Count -eq 0 )
	$Results.Add( [pscustomobject]@{ Id = $Id; Pass = $isPass; Failures = $Failures; Notes = $Notes } )
}

function Read-Text( [string]$RelPath )
{
	$szFull = Join-Path $Root $RelPath
	if( -not ( Test-Path -LiteralPath $szFull ) ) {
		return $null
	}
	return [System.IO.File]::ReadAllText( $szFull, [System.Text.Encoding]::UTF8 )
}

function Read-Lines( [string]$RelPath )
{
	$szText = Read-Text $RelPath
	if( $null -eq $szText ) {
		return $null
	}
	return $szText -split "\r?\n"
}

function Get-LineCount( [string]$RelPath )
{
	$lineArray = Read-Lines $RelPath
	if( $null -eq $lineArray ) {
		return 0
	}

	# split 在結尾換行後多出一個空字串
	if( $lineArray.Count -gt 0 -and $lineArray[ -1 ] -eq '' ) {
		return $lineArray.Count - 1
	}
	return $lineArray.Count
}

function Test-ApplyToAll( [string]$RelPath )
{
	$szText = Read-Text $RelPath
	if( $null -eq $szText ) {
		return $false
	}
	return $szText -match '(?s)\A---\s*\r?\n.*?applyTo:\s*"\*\*".*?\r?\n---'
}

function Test-HasFrontmatterApplyTo( [string]$RelPath )
{
	$szText = Read-Text $RelPath
	if( $null -eq $szText ) {
		return $false
	}
	return $szText -match '(?s)\A---\s*\r?\n.*?applyTo:.*?\r?\n---'
}

function Get-RelPath( [string]$FullPath )
{
	$szRel = $FullPath.Substring( $Root.Length ).TrimStart( '\', '/' )
	return $szRel -replace '\\', '/'
}

function Get-Files( [string]$RelDir, [string]$Filter, [switch]$Recurse )
{
	$szDir = Join-Path $Root $RelDir
	if( -not ( Test-Path -LiteralPath $szDir ) ) {
		return @()
	}
	$itemArray = Get-ChildItem -LiteralPath $szDir -Filter $Filter -File -Recurse:$Recurse
	return @( $itemArray | ForEach-Object { Get-RelPath $_.FullName } )
}

# 規範檔：本議題新增與修改、需符合新規範的檔案
function Get-GuideFiles()
{
	$fileList = @(
		'.github/copilot-instructions.md',
		'CLAUDE.md',
		'.github/instructions/collaboration.instructions.md',
		'.github/instructions/project-overview.instructions.md',
		'.github/instructions/coding-style-general.instructions.md',
		'.github/project/build.md'
	)
	$fileList += Get-Files '.github/process' '*.md'
	$fileList += Get-Files '.claude/skills' 'SKILL.md' -Recurse
	return $fileList
}

# 以檔案實際名稱逐層比對，確認存在且大小寫一致
function Test-PathExact( [string]$RelPath )
{
	$szCurrent = $Root
	foreach( $szPart in ( $RelPath -split '[\\/]' ) ) {
		if( $szPart -eq '' -or $szPart -eq '.' ) {
			continue
		}
		if( $szPart -eq '..' ) {
			$szCurrent = Split-Path $szCurrent -Parent
			continue
		}
		$item = Get-ChildItem -LiteralPath $szCurrent -Force | Where-Object { $_.Name -ceq $szPart }
		if( $null -eq $item ) {
			return $false
		}
		$szCurrent = Join-Path $szCurrent $szPart
	}
	return $true
}

# 依標題切分 Markdown，回傳 @{ Title; Lines }
function Get-Sections( [string[]]$LineArray )
{
	$sectionList = New-Object System.Collections.Generic.List[object]
	$current = [pscustomobject]@{ Title = ''; Lines = New-Object System.Collections.Generic.List[string] }
	foreach( $szLine in $LineArray ) {
		if( $szLine -match '^#{1,6}\s+(.*)$' ) {
			$sectionList.Add( $current )
			$current = [pscustomobject]@{ Title = $Matches[ 1 ]; Lines = New-Object System.Collections.Generic.List[string] }
			continue
		}
		$current.Lines.Add( $szLine )
	}
	$sectionList.Add( $current )
	return $sectionList
}

# ============================================================
# TEST-1 常駐行數
# ============================================================

function Get-BaseResidentLineCount()
{
	# 比對基準的常駐內容：入口文件與當時 applyTo: "**" 的指引
	$fileList = @( '.github/copilot-instructions.md', 'CLAUDE.md' )
	$fileList += @( git ls-tree --name-only "$Base" '.github/instructions/' ) | Where-Object { $_ -like '*.md' }
	$nTotal = 0
	foreach( $szFile in ( $fileList | Sort-Object -Unique ) ) {
		$lineArray = @( git show "${Base}:$szFile" )
		$isEntry = ( $szFile -eq 'CLAUDE.md' ) -or ( $szFile -eq '.github/copilot-instructions.md' )
		$isResident = ( $lineArray -join "`n" ) -match '(?s)\A---\s*\n.*?applyTo:\s*"\*\*".*?\n---'
		if( $isEntry -or $isResident ) {
			$nTotal += $lineArray.Count
		}
	}
	return $nTotal
}

function Test-1()
{
	$failList = @()
	$residentList = @( '.github/copilot-instructions.md', 'CLAUDE.md' )
	$residentList += @( Get-Files '.github/instructions' '*.md' | Where-Object { Test-ApplyToAll $_ } )

	$nTotal = 0
	$noteList = @()
	foreach( $szFile in $residentList ) {
		$nLines = Get-LineCount $szFile
		$nTotal += $nLines
		$noteList += "$szFile = $nLines"
	}
	# 行數只是觀察指標（DEC-11）：輸出與基準的差異，增加時由人在 Review.md 確認理由
	$nBaseTotal = Get-BaseResidentLineCount
	$nDiff = $nTotal - $nBaseTotal
	$noteList += "總計 = $nTotal（基準 $nBaseTotal，差異 $nDiff）"
	if( $nDiff -gt 0 ) {
		$noteList += '常駐總行數增加，Review.md 須寫明增加量與理由'
	}

	$szCollab = '.github/instructions/collaboration.instructions.md'
	if( -not ( Test-Path -LiteralPath ( Join-Path $Root $szCollab ) ) ) {
		$failList += "缺少 $szCollab"
	}
	Add-Result 'TEST-1' $failList $noteList
}

# ============================================================
# TEST-2 團隊通用層不含專案內容
# ============================================================

function Test-2()
{
	$failList = @()
	$termArray = @( 'MyCAM', 'Build.bat', 'CopyRef', 'FCLTool', 'PostTool', 'OCC', 'LanguageManager',
		'Designer', 'reference/', '.worktreeinclude', 'CustPost', 'zh-TW' )
	$fileList = @( '.github/instructions/collaboration.instructions.md' ) + @( Get-Files '.github/process' '*.md' )
	foreach( $szFile in $fileList ) {
		$lineArray = Read-Lines $szFile
		if( $null -eq $lineArray ) {
			$failList += "缺少 $szFile"
			continue
		}
		for( $i = 0; $i -lt $lineArray.Count; $i++ ) {
			foreach( $szTerm in $termArray ) {
				if( $lineArray[ $i ].Contains( $szTerm ) ) {
					$failList += "${szFile}:$( $i + 1 ) 含 '$szTerm'"
				}
			}
		}
	}
	if( ( Get-Files '.github/process' '*.md' ).Count -lt 4 ) {
		$failList += '.github/process/ 不足四份文件'
	}
	Add-Result 'TEST-2' $failList
}

# ============================================================
# TEST-3 規則只寫一處
# ============================================================

function Test-3()
{
	$failList = @()
	$noteList = @()
	$signatureArray = @( '1–5 合併', '必修 / 建議 / 提問', '_Alt2', '前綴與內容間空一格', '完成定義' )
	$fileList = @( Get-Files '.github' '*.md' -Recurse ) + @( 'CLAUDE.md' ) + @( Get-Files '.claude/skills' 'SKILL.md' -Recurse )
	foreach( $szSig in $signatureArray ) {
		$hitList = @( $fileList | Where-Object { ( Read-Text $_ ).Contains( $szSig ) } )
		$noteList += "'$szSig' → $( $hitList -join ', ' )"
		if( $hitList.Count -ne 1 ) {
			$failList += "'$szSig' 出現在 $( $hitList.Count ) 個檔案"
		}
	}

	# SKILL.md 本文只能是指向 process/ 的說明與必讀清單
	$skillList = @( Get-Files '.claude/skills' 'SKILL.md' -Recurse )
	if( $skillList.Count -lt 3 ) {
		$failList += ".claude/skills/ 只有 $( $skillList.Count ) 個 SKILL.md"
	}
	foreach( $szSkill in $skillList ) {
		$szBody = ( Read-Text $szSkill ) -replace '(?s)\A---.*?\r?\n---\s*', ''
		$bodyLineArray = @( $szBody -split "\r?\n" | Where-Object { $_.Trim() -ne '' } )
		if( -not $szBody.Contains( '.github/process/' ) ) {
			$failList += "$szSkill 本文沒有指向 .github/process/"
		}
		if( $bodyLineArray.Count -gt 10 ) {
			$failList += "$szSkill 本文 $( $bodyLineArray.Count ) 行，疑似含規則內容"
		}
	}
	Add-Result 'TEST-3' $failList $noteList
}

# ============================================================
# TEST-4 載入設定與檔案位置
# ============================================================

function Test-4()
{
	$failList = @()
	$szCollab = '.github/instructions/collaboration.instructions.md'

	# 4.1
	if( -not ( Test-ApplyToAll $szCollab ) ) {
		$failList += "$szCollab 的 frontmatter 不是 applyTo: `"**`""
	}

	# 4.2
	$onDemandList = @( Get-Files '.github/process' '*.md' ) + @( '.github/project/build.md' )
	foreach( $szFile in $onDemandList ) {
		if( -not ( Test-Path -LiteralPath ( Join-Path $Root $szFile ) ) ) {
			$failList += "缺少 $szFile"
		}
		elseif( Test-HasFrontmatterApplyTo $szFile ) {
			$failList += "$szFile 不應有 applyTo"
		}
	}
	if( @( Get-Files '.github/instructions/process' '*.md' -Recurse ).Count -gt 0 ) {
		$failList += 'process 文件不應放在 instructions/ 下'
	}

	# 4.3
	$expectArray = @( '.github/copilot-instructions.md', '.github/instructions/collaboration.instructions.md',
		'.github/instructions/project-overview.instructions.md', '.github/instructions/coding-style-general.instructions.md' )
	$refArray = @( Read-Lines 'CLAUDE.md' | Where-Object { $_ -match '^@' } | ForEach-Object { $_.Substring( 1 ).Trim() } )
	$diff = Compare-Object -ReferenceObject $expectArray -DifferenceObject $refArray
	if( $null -ne $diff ) {
		$failList += "CLAUDE.md 的 @ 引用不符：$( $refArray -join ', ' )"
	}

	# 4.4
	$szCopilot = Read-Text '.github/copilot-instructions.md'
	$needArray = @( 'collaboration.instructions.md', 'issue-workflow.md', 'issue-docs.md', 'review.md',
		'git-workflow.md', 'project/build.md', '常駐', '依時機讀取' )
	foreach( $szNeed in $needArray ) {
		if( -not $szCopilot.Contains( $szNeed ) ) {
			$failList += "copilot-instructions.md 缺少 '$szNeed'"
		}
	}

	# 4.5
	$szCollabText = Read-Text $szCollab
	if( $null -eq $szCollabText -or -not $szCollabText.Contains( '新增或修改程式碼' ) ) {
		$failList += "$szCollab 的流程索引缺少「新增或修改程式碼」"
	}

	# 4.6
	$szIgnore = ( git check-ignore -v SESSION.md 2>$null ) -join ''
	if( $szIgnore -notmatch '^\.gitignore:\d+:/SESSION\.md' ) {
		$failList += "SESSION.md 未由 .gitignore 的 /SESSION.md 排除（$szIgnore）"
	}
	Add-Result 'TEST-4' $failList
}

# ============================================================
# TEST-5 異動範圍符合清單
# ============================================================

function Test-5()
{
	$failList = @()
	$allowArray = @(
		'.github/instructions/collaboration.instructions.md',
		'.github/process/issue-workflow.md',
		'.github/process/issue-docs.md',
		'.github/process/review.md',
		'.github/process/git-workflow.md',
		'.github/instructions/project-overview.instructions.md',
		'.github/project/build.md',
		'.github/instructions/coding-style-general.instructions.md',
		'.github/copilot-instructions.md',
		'CLAUDE.md',
		'.gitignore',
		'.editorconfig',
		'.claude/skills/issue-start/SKILL.md',
		'.claude/skills/review/SKILL.md',
		'.claude/skills/release-note/SKILL.md'
	)

	# 已提交、未提交與未追蹤的檔案都要納入
	$changedList = @( git diff --name-only $Base )
	$changedList += @( git ls-files --others --exclude-standard )
	foreach( $szFile in ( $changedList | Sort-Object -Unique ) ) {
		$isAllowed = ( $allowArray -contains $szFile ) -or $szFile.StartsWith( 'Doc/AUTO-TBD/' )
		if( -not $isAllowed ) {
			$failList += "清單外的異動：$szFile"
		}
	}

	# 反向：清單內的檔案都要有異動
	foreach( $szFile in $allowArray ) {
		if( $changedList -notcontains $szFile ) {
			$failList += "清單內的檔案沒有異動：$szFile"
		}
	}
	Add-Result 'TEST-5' $failList
}

# ============================================================
# TEST-6 格式與數字條列
# ============================================================

function Test-6()
{
	$failList = @()
	$noteList = @()
	$fileList = @( Get-GuideFiles ) + @( Get-Files 'Doc/AUTO-TBD' '*.md' ) + @( 'Doc/AUTO-TBD/Check.ps1' )

	# 只在簡體中文出現的常用字
	$simplifiedArray = @( '这', '们', '说', '时', '为', '发', '会', '过', '对', '还', '进', '个', '来', '实', '规', '档' )

	foreach( $szFile in $fileList ) {
		$szFull = Join-Path $Root $szFile
		if( -not ( Test-Path -LiteralPath $szFull ) ) {
			$failList += "缺少 $szFile"
			continue
		}

		# 6.1 編碼：.md 無 BOM、.ps1 有 BOM
		$byteArray = [System.IO.File]::ReadAllBytes( $szFull )
		$hasBom = $byteArray.Count -ge 3 -and $byteArray[ 0 ] -eq 0xEF -and $byteArray[ 1 ] -eq 0xBB -and $byteArray[ 2 ] -eq 0xBF
		$isPs1 = $szFile.EndsWith( '.ps1' )
		if( $isPs1 -and -not $hasBom ) {
			$failList += "$szFile 應為 UTF-8 with BOM"
		}
		elseif( -not $isPs1 -and $hasBom ) {
			$failList += "$szFile 應為 UTF-8 without BOM"
		}

		# 6.1 換行：以 index 與 attr 判定
		$szEol = ( git ls-files --eol -- $szFile ) -join ''
		if( $szEol -eq '' ) {
			$noteList += "$szFile 未追蹤，換行待提交後檢查"
		}
		elseif( $szEol -notmatch 'i/lf' -or $szEol -notmatch 'eol=crlf' ) {
			$failList += "$szFile 換行設定不符：$szEol"
		}

		# 6.3 檔名
		$szName = Split-Path $szFile -Leaf
		if( $szName -notmatch '^[A-Za-z0-9_.\-]+$' ) {
			$failList += "$szFile 檔名不是英文"
		}

		if( $isPs1 ) {
			continue
		}

		# 6.2 程式碼區塊外沒有無序號條列；6.3 沒有簡體字
		$lineArray = Read-Lines $szFile
		$isInCode = $false
		for( $i = 0; $i -lt $lineArray.Count; $i++ ) {
			$szLine = $lineArray[ $i ]
			if( $szLine -match '^\s*```' ) {
				$isInCode = -not $isInCode
				continue
			}
			if( $isInCode ) {
				continue
			}
			if( $szLine -match '^\s*[-*]\s' ) {
				$failList += "${szFile}:$( $i + 1 ) 無序號條列"
			}
			foreach( $szChar in $simplifiedArray ) {
				if( $szLine.Contains( $szChar ) ) {
					$failList += "${szFile}:$( $i + 1 ) 含簡體字 '$szChar'"
				}
			}
		}
	}
	Add-Result 'TEST-6' $failList $noteList
}

# ============================================================
# TEST-7 連結與引用有效
# ============================================================

function Test-7()
{
	$failList = @()
	$fileList = @( Get-Files '.github' '*.md' -Recurse ) + @( 'CLAUDE.md' ) +
		@( Get-Files '.claude/skills' 'SKILL.md' -Recurse ) + @( Get-Files 'Doc/AUTO-TBD' '*.md' )
	foreach( $szFile in $fileList ) {
		$szText = Read-Text $szFile
		$szDir = Split-Path $szFile -Parent

		# 相對連結：以檔案所在目錄為基準
		foreach( $match in [regex]::Matches( $szText, '\]\(([^)\s]+)\)' ) ) {
			$szLink = $match.Groups[ 1 ].Value -replace '#.*$', ''
			if( $szLink -eq '' -or $szLink -match '^[a-z]+:' ) {
				continue
			}
			$szTarget = if( $szDir ) { "$szDir/$szLink" } else { $szLink }
			if( -not ( Test-PathExact $szTarget ) ) {
				$failList += "${szFile}：連結 $szLink 不存在"
			}
		}

		# @ 引用與反引號內的 .github/ 路徑：以 repo 根目錄為基準
		$refList = @()
		foreach( $match in [regex]::Matches( $szText, '(?m)^@(\S+)' ) ) {
			$refList += $match.Groups[ 1 ].Value
		}
		# review 報告的位置寫成 `檔案:行號`，只檢查檔案部分
		foreach( $match in [regex]::Matches( $szText, '`(\.github/[^`\s:]+)(:[0-9-]+)?`' ) ) {
			$refList += $match.Groups[ 1 ].Value
		}
		foreach( $szRef in $refList ) {
			# 範例路徑（含萬用字元、佔位符、省略號）不檢查
			if( $szRef -match '[*<>]|\.\.\.' ) {
				continue
			}
			if( -not ( Test-PathExact $szRef ) ) {
				$failList += "${szFile}：路徑 $szRef 不存在"
			}
		}
	}
	Add-Result 'TEST-7' ( @( $failList | Sort-Object -Unique ) )
}

# ============================================================
# TEST-9 專案指引與 Coding Style 修訂
# ============================================================

function Test-9()
{
	$failList = @()
	$szOverview = '.github/instructions/project-overview.instructions.md'
	$lineArray = Read-Lines $szOverview
	$szText = Read-Text $szOverview

	# 9.1
	if( $szText -match '(?m)^#+\s*Git 流程' ) {
		$failList += "$szOverview 仍有「Git 流程」章節"
	}
	foreach( $szTerm in @( 'FakeDebug', 'vswhere', 'MachineData' ) ) {
		if( $szText.Contains( $szTerm ) ) {
			$failList += "$szOverview 仍有建置細節 '$szTerm'"
		}
	}
	if( -not $szText.Contains( 'project/build.md' ) ) {
		$failList += "$szOverview 沒有指向 project/build.md 的索引"
	}
	$sectionList = Get-Sections $lineArray
	$check = $sectionList | Where-Object { $_.Title -match '專案固定檢查項目' } | Select-Object -First 1
	if( $null -eq $check ) {
		$failList += "$szOverview 沒有「專案固定檢查項目」章節"
	}
	else {
		$nItemCount = @( $check.Lines | Where-Object { $_ -match '^\d+\.\s' } ).Count
		if( $nItemCount -ne 5 ) {
			$failList += "「專案固定檢查項目」有 $nItemCount 項，應為 5 項"
		}
	}

	# 9.2
	$szBuild = Read-Text '.github/project/build.md'
	if( $null -eq $szBuild ) {
		$failList += '缺少 .github/project/build.md'
	}
	else {
		foreach( $szHead in @( 'FCLTool', '命令列建置', '執行期檔案', 'worktree 準備' ) ) {
			if( $szBuild -notmatch "(?m)^#+\s.*$( [regex]::Escape( $szHead ) )" ) {
				$failList += "build.md 缺少「$szHead」章節"
			}
		}
	}

	# 9.3 三項規則只出現在「專案固定檢查項目」
	$ruleMap = [ordered]@{ '四語系同步' = '語系.*同步|同步.*語系'; 'Designer 可開' = 'Designer.*開啟'; '不修改 reference/' = 'reference/.*修改|修改.*reference/' }
	foreach( $section in $sectionList ) {
		if( $section.Title -match '專案固定檢查項目' ) {
			continue
		}
		foreach( $szKey in $ruleMap.Keys ) {
			if( @( $section.Lines | Where-Object { $_ -match $ruleMap[ $szKey ] } ).Count -gt 0 ) {
				$failList += "「$szKey」重複出現在「$( $section.Title )」"
			}
		}
	}

	# 9.4
	$szGeneral = Read-Text '.github/instructions/coding-style-general.instructions.md'
	if( $szGeneral.Contains( '維持該方法內既有的命名方式' ) ) {
		$failList += 'coding-style-general 仍有「維持該方法內既有的命名方式」'
	}
	foreach( $szNeed in @( '採新規範', '整個方法', '獨立 commit', '沒有動到的方法', 'AUTO-<編號>' ) ) {
		if( -not $szGeneral.Contains( $szNeed ) ) {
			$failList += "coding-style-general 缺少 '$szNeed'"
		}
	}
	Add-Result 'TEST-9' $failList
}

# ============================================================
# TEST-11 本議題文件與 commit
# ============================================================

function Test-11()
{
	$failList = @()
	$noteList = @()

	# 11.1
	foreach( $szDoc in @( 'README', 'Goal', 'Spec', 'TestPlan', 'Implementation', 'Review', 'Discussion', 'FollowUp' ) ) {
		if( -not ( Test-Path -LiteralPath ( Join-Path $Root "Doc/AUTO-TBD/$szDoc.md" ) ) ) {
			$failList += "缺少 Doc/AUTO-TBD/$szDoc.md"
		}
	}
	$szReadme = Read-Text 'Doc/AUTO-TBD/README.md'
	foreach( $szHead in @( '文件', 'DoD', 'Release Note' ) ) {
		if( $szReadme -notmatch "(?m)^##\s.*$( [regex]::Escape( $szHead ) )" ) {
			$failList += "README.md 缺少「$szHead」章節"
		}
	}
	$nChecked = ( [regex]::Matches( $szReadme, '(?m)^\s*\d+\.\s\[x\]' ) ).Count
	$nUnchecked = ( [regex]::Matches( $szReadme, '(?m)^\s*\d+\.\s\[ \]' ) ).Count
	if( $nChecked -eq 0 -or $nUnchecked -gt 0 ) {
		$failList += "README.md 的 DoD 未全部勾選（已勾 $nChecked、未勾 $nUnchecked）"
	}
	$readmeSectionList = Get-Sections ( $szReadme -split "\r?\n" )
	$release = $readmeSectionList | Where-Object { $_.Title -match 'Release Note' } | Select-Object -First 1
	if( $null -ne $release ) {
		$noteLineArray = @( $release.Lines | Where-Object { $_ -match '^\d+\.\s' } )
		if( $noteLineArray.Count -eq 0 ) {
			$failList += 'Release Note 沒有條列'
		}
		foreach( $szLine in $noteLineArray ) {
			if( $szLine -notmatch '^\d+\.\s(新增|修改|刪除) \S' ) {
				$failList += "Release Note 前綴不符：$szLine"
			}
		}
	}

	# 11.2
	$szReview = Read-Text 'Doc/AUTO-TBD/Review.md'
	if( $null -ne $szReview ) {
		foreach( $szField in @( '議題', '分支', '比對基準', '工具與模型', '日期' ) ) {
			if( -not $szReview.Contains( $szField ) ) {
				$failList += "Review.md 表頭缺少「$szField」"
			}
		}
		if( $szReview -notmatch '\|\s*REV\s*\|\s*段\s*\|\s*嚴重度\s*\|\s*位置\s*\|\s*說明\s*\|\s*處置\s*\|' ) {
			$failList += 'Review.md 發現表格欄位不是 REV、段、嚴重度、位置、說明、處置'
		}
		foreach( $match in [regex]::Matches( $szReview, '(?m)^\|\s*REV-\d+\s*\|.*$' ) ) {
			$cellArray = $match.Value.Trim( '|' ) -split '\|' | ForEach-Object { $_.Trim() }
			$szSeverity = $cellArray[ 2 ]
			$szAction = $cellArray[ -1 ]
			if( $szAction -eq '' -or $szAction -eq '待處置' ) {
				$failList += "$( $cellArray[ 0 ] ) 沒有處置"
			}
			elseif( $szAction.Contains( '待人確認' ) ) {
				$failList += "$( $cellArray[ 0 ] ) 的處置待人確認"
			}
			elseif( $szSeverity -eq '必修' -and -not $szAction.StartsWith( '已修正' ) ) {
				$failList += "$( $cellArray[ 0 ] ) 為必修但處置不是已修正"
			}
		}
	}

	# 11.3 施工 commit 須標示 STEP 或 REV；只動議題文件（.md）的 commit 屬確認點文件
	$hashArray = @( git log --format=%H "$Base..HEAD" )
	$stepSet = @{}
	foreach( $szHash in $hashArray ) {
		$szSubject = ( git log -1 --format=%s $szHash ).Trim()
		$fileArray = @( git diff-tree --no-commit-id --name-only -r $szHash )
		$isDocOnly = @( $fileArray | Where-Object { $_ -notmatch '^Doc/AUTO-TBD/[^/]+\.md$' } ).Count -eq 0
		$tagMatch = [regex]::Match( $szSubject, '\(((STEP|REV)-\d+(,\s*(STEP|REV)-\d+)*)\)$' )
		if( $tagMatch.Success ) {
			foreach( $stepMatch in [regex]::Matches( $tagMatch.Value, 'STEP-(\d+)' ) ) {
				$stepSet[ [int]$stepMatch.Groups[ 1 ].Value ] = $true
			}
		}
		elseif( -not $isDocOnly ) {
			$failList += "commit $( $szHash.Substring( 0, 8 ) ) 未標示 STEP 或 REV：$szSubject"
		}
	}
	$nStepCount = ( [regex]::Matches( ( Read-Text 'Doc/AUTO-TBD/Implementation.md' ), '(?m)^###\s+STEP-\d+' ) ).Count
	for( $i = 1; $i -le $nStepCount; $i++ ) {
		if( -not $stepSet.ContainsKey( $i ) ) {
			$failList += "STEP-$i 沒有 commit"
		}
	}
	$noteList += '純格式調整是否獨立 commit 由人工確認'
	Add-Result 'TEST-11' $failList $noteList
}

# ============================================================
# TEST-12 個人設定收斂
# ============================================================

function Test-12()
{
	$failList = @()
	$szHomeClaude = Join-Path $env:USERPROFILE '.claude'
	if( $SkipPersonal ) {
		Add-Result 'TEST-12' @() @( '略過：-SkipPersonal（個人設定只在實作者的電腦檢查）' )
		return
	}

	# 12.1
	if( Test-Path -LiteralPath ( Join-Path $szHomeClaude 'issue-workflow.md' ) ) {
		$failList += '~/.claude/issue-workflow.md 仍存在'
	}
	$szGlobal = [System.IO.File]::ReadAllText( ( Join-Path $szHomeClaude 'CLAUDE.md' ), [System.Text.Encoding]::UTF8 )
	foreach( $szTerm in @( '開發議題處理流程', 'issue-workflow.md', '數字標號' ) ) {
		if( $szGlobal.Contains( $szTerm ) ) {
			$failList += "~/.claude/CLAUDE.md 仍有 '$szTerm'"
		}
	}

	# 12.2 主 checkout 為 git worktree list 的第一筆
	$szMainLine = @( git worktree list --porcelain ) | Where-Object { $_ -match '^worktree ' } | Select-Object -First 1
	$szMain = $szMainLine.Substring( 'worktree '.Length )
	$szLocalPath = Join-Path $szMain 'CLAUDE.local.md'
	if( -not ( Test-Path -LiteralPath $szLocalPath ) ) {
		$failList += "找不到 $szLocalPath"
	}
	else {
		$szLocal = [System.IO.File]::ReadAllText( $szLocalPath, [System.Text.Encoding]::UTF8 )
		foreach( $szTerm in @( '議題資料夾放在', '推送前一律先問我' ) ) {
			if( $szLocal.Contains( $szTerm ) ) {
				$failList += "CLAUDE.local.md 仍有 '$szTerm'"
			}
		}
		foreach( $szTerm in @( 'origin', 'github' ) ) {
			if( -not $szLocal.Contains( $szTerm ) ) {
				$failList += "CLAUDE.local.md 缺少 '$szTerm' 的 remote 用法"
			}
		}
	}
	Add-Result 'TEST-12' $failList
}

# ============================================================
# 執行與輸出
# ============================================================

$testArray = @( 'Test-1', 'Test-2', 'Test-3', 'Test-4', 'Test-5', 'Test-6', 'Test-7', 'Test-9', 'Test-11', 'Test-12' )
foreach( $szTest in $testArray ) {
	try {
		& $szTest
	}
	catch {
		Add-Result ( $szTest.ToUpper() ) @( "執行錯誤：$( $_.Exception.Message )" )
	}
}

Write-Output "比對基準：$Base"
foreach( $result in $Results ) {
	$szState = if( $result.Pass ) { 'PASS' } else { 'FAIL' }
	Write-Output "$( $result.Id ) $szState"
	foreach( $szFail in $result.Failures ) {
		Write-Output "    x $szFail"
	}
	foreach( $szNote in $result.Notes ) {
		Write-Output "    i $szNote"
	}
}
$nFailCount = @( $Results | Where-Object { -not $_.Pass } ).Count
Write-Output "總結：$( $Results.Count - $nFailCount ) PASS、$nFailCount FAIL"
exit $nFailCount
