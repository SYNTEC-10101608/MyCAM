# Copilot Instructions

## Project Guidelines
- 使用者會用 zh-tw 下提示詞，你可以用 en-us 執行或思考，但請用 zh-tw 給出最終回答。

## CAMPoint / PostPoint 架構參考（五軸）

以下為目前實作的導覽，供團隊及 AI 使用；修改前請核對對應程式碼。不涵蓋 ArticRobotPostPoint，也不代表已完成的重構目標。

### 1. 術語與路徑邊界

| 術語 | 意義與歸屬 |
| --- | --- |
| MainPath | 主加工路徑 |
| LeadIn / LeadOut | 路徑內的引入／引出線，**不是空移** |
| OverCut | 路徑內過切段 |
| Entry / Exit | 整體第一條加工路徑前的進刀／最後一條加工路徑後的退刀，屬於空移 |
| Traverse | 前一路徑結束到當前路徑開始的空移，包含抬刀、移動及下刀 |

- 加工段順序：`LeadIn → MainPath → OverCut → LeadOut`。
- 加工起點：優先取 LeadIn 第一點，否則取 MainPath 第一點。
- 加工終點：優先取 LeadOut 最後一點，其次 OverCut 最後一點，否則取 MainPath 最後一點。
- 空移連接上述「加工起終點」，不一定是 MainPath 起終點；取得規則集中於 CacheHelper。

### 2. 資料模型

| 型別 | 內容與定位 |
| --- | --- |
| CADPoint | 幾何位置、兩個法向及切向 |
| CAMPoint | 包裝複製後的 CADPoint，另有初始／目前刀向、初始／修改後旋轉角、傾角、微接點與索引等資訊；不只是 XYZ |
| PostPoint | 五軸後處理／模擬容器：XYZ、Master/Slave、TiltAngle_deg、微接點起終標記 |
| PostData | **單一路徑**加工點列，加上「到達該路徑之前」的空移資料 |
| List<PostData> | 整體多條加工路徑的後處理資料 |

- CAMPoint 沒有加工／空移分類欄位，用途由清單與流程決定；也用於參考點、候選起點等非實際行走點。
- CAMPoint 的 InitMaster_rad / InitSlave_rad / ModMaster_rad / ModSlave_rad，以及 PostPoint 的 Master / Slave，均為 **radian**；TiltAngle_deg 為 degree。NC 寫出時才轉換旋轉角度為 degree 並映射軸名。
- **同一 PostPoint 型別同時承載 G54 路徑座標及模擬 FK 補償後的中間值**，型別沒有區分階段，必須由來源及流程判斷。後者雖在程式中命名為 MCS，仍須於影格階段加上 SimulationOffset 並組合機構鏈；詳見「FK / IK 責任與運算鏈（五軸）」章節。
- Clone 會建立新物件；以 IProcessPoint / IOrientationPoint / ISetToolVecPoint 傳遞，不代表底層不是 CAMPoint。

### 3. CAMPoint 建構與生命週期

#### 加工路徑：由 PathCache 組織建構

| 資料 | 組織者／實際建構者 |
| --- | --- |
| 輪廓 MainPath | ContourCache.BuildCAMPointList：CADPoint 轉 CAMPoint，處理起點、方向、閉合、刀向及角度 |
| 標準圖形 MainPath | 各圖形 Cache → StdPatternCacheBase.SetPathCAMPoint / ConvertToCAMPoints；角度取自參考點 |
| LeadIn / LeadOut | Cache → LeadHelper.SetLeadIn / SetLeadOut → BuildLeadLinePoint；繼承相接加工點的刀向與角度 |
| OverCut | Cache → OverCutHelper.SetOverCut / BuildOverCutPoint；繼承主路徑末點的刀向與角度 |
| 微接點補點 | ContourCache → MicroJointHelper；必要時插入 CAMPoint 並插補修改後角度 |

- Cache 初建時產點；幾何／加工參數變動後標記 dirty，後續讀取相關屬性時依實作重建，不是只在新增路徑時產生一次。
- 主要加工點的 IK 已在 **CAMPoint 階段**完成：輪廓由 ContourCache.SolveInitIK 等刀向處理流程計算；標準圖形由 SolveRefPntInitIK 求參考角度再套用。
- 輪廓可有 LeadIn 與 LeadOut；目前標準圖形 Cache 建立 LeadIn，但 StdPatternCacheBase.LeadOutPointList 回傳空清單，不能假設兩者相同。

##### LeadIn / LeadOut / OverCut 的屬性來源

三者皆建立新的 CADPoint / CAMPoint，**不是完整 Clone 參考點**；以「新幾何＋繼承相接端點姿態」組成加工點，不重新逐點求 IK。

| 路徑段 | 參考點（下表的姿態來源） |
| --- | --- |
| 輪廓 LeadIn | MainPath 第一點 |
| 標準圖形 LeadIn | 排序後的 `m_StartCAMPointList.First()` |
| LeadOut | 有 OverCut 時取其最後一點，否則取 MainPath 最後一點 |
| OverCut | MainPath 最後一點；整段過切都以此點為姿態依據 |

| CAMPoint 資料 | LeadIn / LeadOut | OverCut |
| --- | --- | --- |
| Point | 依 Lead 參數建立的直線／圓弧離散位置 | 從主路徑末點接續主路徑開頭的幾何，依過切長度取點；終點可能在線段內補點 |
| NormalVec_1st / NormalVec_2nd | 複製參考點的兩個法向 | 複製 MainPath 最後一點的兩個法向 |
| TangentVec | 引刀線自身的直線方向或圓弧切向 | 首點用主路徑末點切向，其餘用對應主路徑位置切向；線段內補點時插補切向 |
| InitToolVec | 建構子依第一法向與 `isToolVecReverse` 建立 | 同左 |
| ToolVec | 指定為參考點目前刀向 | 整段指定為 MainPath 最後一點目前刀向 |
| InitMaster_rad / InitSlave_rad | 複製參考點初始角度 | 複製 MainPath 最後一點初始角度 |
| ModMaster_rad / ModSlave_rad | 複製參考點修改後角度 | 複製 MainPath 最後一點修改後角度 |
| TiltAngle_deg | Helper 未設定；稍後由 Cache 更新 | 同左 |
| IsToolVecModPoint / IsMicroJointStart / IsMicroJointEnd | 不複製參考點標記，維持預設 false | 同左 |
| InitPathIndex | 未指定，維持預設 0；不可當作有效原始點索引 | 同左 |

- Lead 幾何依據相接點的位置、切向、ToolVec，以及長度、角度、直線／圓弧、路徑方向與引刀方向設定；位置與切向可改變，但刀向與旋轉角度沿用參考點。
- OverCut 的**幾何與姿態來源不同**：位置／切向沿主路徑開頭取用，法向／ToolVec／旋轉角度整段取自主路徑末點，不能假設逐點複製主路徑對應姿態。末端補點呼叫 InterpolateVecBetween2Point 時，只使用插補切向，未採用其刀向輸出。
- `new CAMPoint(cadPoint, isToolVecReverse)` 以第一法向（依設定反向）建立 InitToolVec，再將 ToolVec 覆寫為參考點目前刀向；**InitToolVec 不是直接複製參考點的同名屬性**。
- Cache 最後呼叫 `ToolVecHelper.UpdateTiltAngles`，以 `Angle(ToolVec, InitToolVec)` 計算 TiltAngle_deg，不直接複製參考點傾角。
- 輪廓建構順序：主路徑刀向／角度計算 → 微接點 → OverCut → LeadIn / LeadOut → 更新傾角。
- 實作入口：LeadHelper.BuildLeadLinePoint / SetLeadPntMS、OverCutHelper.BuildOverCutPoint / SetOverPntMS、PointData.cs 的 CAMPoint 建構子、ToolVecHelper.UpdateTiltAngles。

#### 空移幾何：由 TraverseHelper 按需計算

TraverseHelper 是幾何計算者；PostTraverseBuilder 是後處理空移資料組裝者，兩者不同。

| 結果 | 格式／建立方法 |
| --- | --- |
| Entry/Exit、LiftUp/CutDown | GetCutDownOrLiftUpPoint → BuildTraversePoint → CAMPoint（以 IProcessPoint 傳遞） |
| 蛙跳中間點 | GetFrogLeapMiddlePoint → BuildTraversePoint → CAMPoint |
| 安全平面投影點 | gp_Pnt，不是 CAMPoint |
| 安全圓柱／球面路徑 | TryCalculateSafeBoundaryTraverse → TryBuildBoundaryPath → List<gp_Pnt>，**此階段已做幾何離散** |

一般 BuildTraversePoint 產生的 CAMPoint 只應當作暫時的「位置＋目前 ToolVec」容器：
- 法向／切向使用預設方向，不具加工幾何意義；InitToolVec 也來自該預設法向。
- 初始／修改後旋轉角、傾角與索引未設定；標記為 false。不可視為完整加工姿態或有效原始點索引。
- 例外：抬刀／下刀距離為零時回傳來源點的 Clone，會保留來源資料。
- 空移 PostPoint 的角度由 PostTraverseBuilder 另行沿用或插補，不能使用暫時空移 CAMPoint 的預設角度。

空移 CAMPoint 建立時機：
- TraverseRenderer 重建空移與 Entry/Exit 顯示。
- TraverseAction.CollectTraverseResults 計算安全面設定預覽範圍。
- PostTraverseBuilder.CalculateTraverse / CalculateEntry / CalculateExit 建立後處理資料。
- FiveAxisSimuPathBuilder.GetExitPathIKPnt 另外建立模擬最後退刀段。

#### 空移結果存放位置

- Helper 以 out TraversePathResult 回傳幾何，包括抬刀／下刀／蛙跳點、安全平面兩投影點、SafeBoundaryPointList 及安全面進出索引。
- 原始空移幾何**沒有放進 PathCache 成為共用點列快取**；顯示、後處理等流程各自呼叫計算。
- 顯示端轉成 AIS，存入 TraverseRenderer.m_TraverseAISDict / m_FrogLeapAISDict。
- 後處理端轉成 PostPoint，放入當前路徑 PostData 或 StdPatternPostData；模擬清單保存在 SimuEditor.m_PostDataList。

### 4. PostData 範圍與 PostPoint 建構

| PostData 分類 | 欄位 |
| --- | --- |
| 加工點列 | LeadInPostPointList / MainPathPostPointList / OverCutPostPointList / LeadOutPostPointList |
| 到達該路徑前的空移 | LiftUpPostPoint / CutDownPostPoint / FrogLeapMidPostPoint |
| 安全平面／安全面 | LiftUpPostSafePlanePoint / CutDownPostSafePlanePoint / SafeBoundaryPostPointList |
| 其他 | ProcessStartPoint / FollowSafeDistance |

- ProcessStartPoint 指向 LeadIn 或 MainPath 第一點，不是額外路徑段；各空移欄位依設定填入，不一定有值。
- **前一路徑抬刀點放在當前路徑 PostData**。例如：PostData[A] = Entry + A 加工段；PostData[B] = A→B 空移 + B 加工段。
- **整體 Exit 不在逐路徑 PostData 中**：NC 與模擬各自另外建立。
- PostData 不直接包含路徑 ID、完整 CraftData/TraverseData、模擬新增細插點、FK 點、機構影格或碰撞結果。

G54 PostPoint 的來源：
- 輪廓：ContourPostHelper.GetPostData → ConvertCAMPntListToPostPntList，轉裝各加工清單的 XYZ、**既有 ModMaster/ModSlave**、傾角與微接點標記；不是重新逐點做 IK。
- 標準圖形：StdPatternPostHelper 將參考點、起點（有 LeadIn 時改取其第一點）轉成 PostPoint。
- 空移：PostTraverseBuilder 使用 Helper 幾何建立 PostPoint，直接指定角度，不逐點做 IK：
  - Entry、CutDown、下刀側安全平面點：取當前加工起點角度。
  - LiftUp、抬刀側安全平面點、最後 Exit：取前一／最後加工終點角度。
  - 蛙跳中間點：前後角度平均。
  - 安全圓柱／球面：接近段保持舊角度、離開段保持新角度，安全面上依累積路長插補。

### 5. 顯示與模擬離散責任（目前分散，尚未統一）

| 空移類型 | 畫面顯示 | 模擬 |
| --- | --- | --- |
| Entry/Exit、抬刀/下刀、直線空移 | Renderer 用兩端點建立 AIS_Line，不額外建立業務層細插點列 | SimulationHelper 離散直線，FiveAxisSimuPathBuilder 補角度 |
| 安全平面 | Helper 算投影關鍵點，Renderer 直接連線 | 沿關鍵點逐段直線細插、補角度 |
| 蛙跳 | Renderer 用三點建立 OCC 圓弧及 AIS_Shape，不自行產生細插點列 | SimulationHelper 離散圓弧，FiveAxisSimuPathBuilder 補角度 |
| 安全圓柱／球面 | Helper 產生離散 gp_Pnt 點列，Renderer 逐段連線 | 使用安全面 PostPoint 點列，再逐弦段直線細插、補角度 |

- 共用的是 TraverseHelper 幾何計算邏輯，**不等於共用同一份結果或完全相同的離散策略**。OCC 顯示內部曲線細分，不等於本專案明確建立的模擬點列。
- 模擬直線：SimulationHelper.DiscretizeLine 產生 List<gp_Pnt> → BuildLinearTraverseIKPnt 建立 XYZ＋Master/Slave 的 PostPoint。
- 模擬蛙跳：DiscretizeArcSegments 產生前後兩半圓弧位置清單 → AddInterpolatedPoints 依累積路長插補角度。
- 模擬細插清單以 List<PostPointBase> 傳遞，五軸元素為 PostPoint；**不先建立細插 CAMPoint，也不重新逐點做 IK**。
- 模擬直線預設位置步長 1.0；圓弧預設最大步長 1.0、弦高 0.01。加工段沿用 Cache 點列，沒有在此階段統一重新細插。
- 安全圓柱／球面幾何離散點也供 NC 逐點輸出；一般直線 NC 使用關鍵點，蛙跳 NC 使用 FROG_LEAP 巨集，不使用模擬新增細插點。

### 6. NC 與模擬資料流

#### NC 輸出

`CAMEditor.ConverNC → NCWriter.ConvertSuccess → 讀取 PathCache / 建立 NCPackage → PostHelper + PostTraverseBuilder → 寫 NC`

- 每次輸出建立所需後處理資料；輪廓加工段逐點寫 G01，不經模擬細插或模擬 FK。
- **標準圖形分流**：使用 StdPatternPostData（參考點、加工起點與空移），由 StdPatternNCWriter 寫 SY_CIRC/SY_RECT/SY_RUNWAY 等巨集及幾何參數，不逐一輸出完整離散加工點。

#### 模擬

1. 進入 SimuEditor.EditStart：SetPostDataList → NCWriter.ConvertPostDataListSuccess → m_PostDataList。
2. 需要重算時：CalSimulationResult → BuildSimuData → FiveAxisSimuPathBuilder.BuildFrameTransMap。
3. GetWholePathIKPnt 串接各路徑前空移、加工段及最後 Exit，補空移細插點，組成 PathSimuIKData。IK 命名表示 FK 前資料，不能據此認定每個點重新求 IK。
4. BuildPathFKList → BuildFKPostPntFiveAxis → PostSolver.SolveFK：對空移、加工、退刀點求 RTCP 補償向量，以「G54 點＋補償」建立 FK PostPoint，組成 PathSimuFKData；SimulationOffset 依構型參與補償計算，但尚未直接加到 XYZ。
5. SimulationHelper.BuildFrame → FiveAxisPerFrameTransformBuilder 建立各元件自身變換（含 SimulationOffset）→ 依機構鏈相乘產生最終 gp_Trsf → 碰撞計算 → MachineRender 播放。碰撞使用這些影格，不另建立碰撞專用細插點。

- 順序是 **G54 點列 → 空移細插與角度插補 → FK → 機構影格**，不是 FK 後才細插。
- 標準圖形模擬另取完整離散加工 PostData，再由 ApplyStdPatternPostDataToPostData 套用標準圖形參考角度與空移資料；與 NC 巨集輸出不同。
- 模擬不是解譯輸出的 NC；不可據此宣稱完整重現控制器巨集、自訂 G-code、速度或加減速插補。
- ToolVectorAction / LockABAction 也建立僅帶 Master/Slave 的暫時 PostPoint，再透過 BuildSinglePointFrameTransform 結合位置與 FK，預覽單點機構姿態。

### 7. 修改界線與已知限制

- 不要混用 LeadIn/LeadOut 與 Entry/Exit；不要將前一路徑抬刀誤放回前一路徑 PostData。
- 不要把暫時空移 CAMPoint 的預設角度當成已求解姿態，也不要把所有 PostPoint 當成同一座標系。
- 目前 FK 新建 PostPoint 只填 XYZ、Master、Slave，未複製傾角／微接點標記；不能視為完整加工 PostPoint 的等價替代物。
- 細插主要依位置距離。位置重合的直線段只保留起終姿態，未依旋轉量加密；原地／大角度旋轉及碰撞精度需另評估，現有取樣不等於連續掃掠碰撞保證。
- 若未來統一離散，可考慮「共用 XYZ 幾何取樣 → 顯示；另補角度／必要姿態加密 → 模擬 FK」，但**這是可能的重構方向，不是現況或既定實作要求**。

### 8. 程式碼索引（相對儲存庫根目錄）

- 型別：MyCAM/Data/PointData.cs、MyCAM/Data/PostData.cs
- 加工 Cache／起終點：MyCAM/PathCache/ 下的 ContourCache.cs、StdPatternCacheBase.cs、各圖形 Cache、CacheHelper.cs
- 加工補點：MyCAM/Helper/CAM/ 下的 LeadHelper.cs、OverCutHelper.cs、MicroJointHelper.cs
- 空移幾何：MyCAM/Helper/CAM/TraverseHelper.cs
- 顯示／設定預覽：MyCAM/Editor/Renderer/TraverseRenderer.cs、MyCAM/Editor/Action/CAM/TraverseAction.cs
- 後處理：MyCAM/Post/ 下的 ContourPostHelper.cs、StdPatternPostHelper.cs、PostTraverseBuilder.cs、PostSolver.cs
- NC：MyCAM/Post/ 下的 NCWriter.cs、NCWriterHelper.cs、StdPatternNCWriter.cs
- 模擬：MyCAM/Editor/SimuEditor.cs、MyCAM/Helper/Simulation/ 下的 FiveAxisSimuPathBuilder.cs、SimulationHelper.cs

## FK / IK 責任與運算鏈（五軸）

以下描述目前實作，不是控制器 RTCP 的通用規格，也不涵蓋關節型機器人。**姿態求解、RTCP 位置補償、機構影格是不同責任**；不要僅依 FK / IK 方法名稱判斷資料階段。

### 1. API 與資料界線

| API / 流程 | 輸入 → 輸出 | 責任 |
| --- | --- | --- |
| PostSolver.SolveIK | ToolVec＋參考 M/S → M/S＋IKSolveResult | 姿態逆解；不求線性軸位置 |
| PostSolver.SolveToolVec | M/S → ToolVec | 姿態正解；不求位置補償 |
| PostSolver.IsToolVecSingular | M/S → bool | 回算 ToolVec，判斷是否接近數學上的主旋轉軸方向 |
| PostSolver.SolveFK | M/S＋G54 點＋offset → gp_Vec | RTCP 位置補償向量；不是絕對 MCS 點或元件最終位置 |
| BuildFKPostPntFiveAxis | G54 PostPoint＋補償向量 → FK PostPoint | 保留 M/S，XYZ 改為 G54 點＋補償 |
| BuildFrame / FKToFrameTranfResult | FK PostPoint＋offset＋機台／機構鏈 → 各元件 gp_Trsf | 建立自身運動變換，再依祖先鏈相乘 |

- PostSolver 的 M/S 輸入輸出均為 **radian**；ToolVecHelper 與編輯 UI 的部分 API 使用 **degree**，須依方法確認轉換。
- 姿態求解使用機台零角度刀向 ToolDir、旋轉軸方向、構型；IK 另使用軸範圍及參考角度選解，不需要加工點 XYZ 或 SimulationOffset。
- PostSolver 依 Spindle / Table / Mix 建立求解器，內部處理 M/S 交換與旋轉慣例；單旋轉軸 Mix 變體會將鎖定軸的 IK 輸出設為 0。呼叫端不要自行重複交換。

### 2. 加工姿態：由 Cache 建立，供顯示與後處理使用

| 使用情境 | 主要呼叫鏈與傳遞資訊 |
| --- | --- |
| 輪廓初始姿態 | ContourCache.BuildCAMPointList → CalMSForEachPoint → SolveInitIK → 逐點 SolvIK → SolveIK(InitToolVec, 前次 M/S) → 寫入 Init/Mod M/S → SolveToolVec 回算目前 ToolVec |
| 標準圖形初始姿態 | 各圖形 Cache → StdPatternCacheBase.SolveRefPntInitIK → SolveIK(參考點 InitToolVec, CraftData 初始 M/S) → 寫入參考點角度，再套用至加工點；不是完整路徑逐點 IK |
| 刀向／局部傾角修改與插補 | ToolVecHelper.SetToolVec 等流程先建立各點 ToolVec → SolveRegionIK / SolveIKAndFilterSingularity → 逐點 SolveIK → 更新 Mod M/S，並處理奇異點 |
| 指定／插補 M/S | SetToolVecByAssignedMS、InterpolateToolVecByMS、單軸插補等先設定 Mod M/S → ConvertMSAngleToToolVec → SolveToolVec |
| 鎖軸 | SetToolVecByLockedAxis 固定其中一個 Mod 角度、保留另一軸 → SolveToolVec 更新 ToolVec；不是重新做鎖軸條件下的 IK |
| 斜切 | SetBevelByStartMS / SetBevelCaseToolVecForAssingAB 建立各點局部傾角對應刀向 → SolveIKAndFilterSingularity |

- 輪廓初始 IK 種子來自 CraftData.InitMaster_rad / InitSlave_rad，後續傳遞上一點解以維持選解連續性；CalMSForEachPoint 後續仍可能依刀向模式再次修改姿態，初始回算結果不一定是最終 ToolVec。
- SolveRefPntInitIK 本身沒有像輪廓 SolvIK 一樣再呼叫 SolveToolVec。
- SolveIK 內部走 IKSolver.Solve → FiveAxisSolver.IJKtoMS → ChooseSolution(ShortestDist)。先依軸範圍選解，失敗後可能不限範圍重選並回傳 OutOfRange；**有輸出角度不代表在行程內或保證安全**，須檢查結果碼。奇異區域亦有容差處理，不應假設輸入刀向與回算刀向完全一致。
- LeadIn / LeadOut / OverCut 沿用相接點姿態；空移角度由 PostTraverseBuilder 沿用或插補，均不能概括為重新逐點 IK。
- NC 寫出使用 Cache 的既有修改後角度；讀取 dirty Cache 可間接觸發重算，但 NC 不經模擬 SolveFK 或機構影格。

### 3. ToolVectorAction：機台 M/S 與局部 AB 傾角的轉換

**此處 AB / RA / RB 是加工點局部傾角，不是機台實體 A/B 軸名。** 局部基底取 X＝TangentVec、Z＝InitToolVec、Y＝Z × X，因此轉換也需要指定加工點；相同 M/S 在不同點可對應不同 AB。

| UI 操作 | 呼叫鏈 |
| --- | --- |
| OnABAngleChanged | GetMSAngleFromABAngle → ConvertABAngleToToolVec → GetMSAngleFromToolVec → SolveIK → M/S |
| OnMSAngleChanged | GetABAngleFromMSAngle → ConvertMSAngleToToolVec → SolveToolVec → GetABAngleFromToolVec → AB |
| 保持前點／指定 Z 刀向 | OnSetKeep / OnSetZDir 透過上述 Helper 取得一致的 M/S 與 AB |
| 翻轉旋轉軸 | ToolVectorAction / LockABAction → FlipRotaryAxis → IsSingular → IsToolVecSingular → SolveToolVec；再依翻轉規則調整角度 |

AB ↔ ToolVec 的局部幾何轉換由 ToolVecHelper 負責；PostSolver 只負責 ToolVec ↔ 機台 M/S。GetMSAngleFromToolVec 依參數選 Init 或 Mod 角度作為 IK 種子，預設使用 Init。

### 4. RTCP 補償與機構位置：三個階段

定義 P＝G54 加工點、O＝machineData.SimulationOffset、Δtcp＝SolveFK(M, S, P, O)。

| 階段 | 計算／輸出 | 注意事項 |
| --- | --- | --- |
| 1. RTCP 位置補償 | BuildFKPostPntFiveAxis 建立 FKPoint.XYZ＝P＋Δtcp，保留 M/S | 程式變數名 pointMCS；尚未直接加 O，也不是各部件最終位置 |
| 2. 元件自身運動 | 線性軸平移輸入＝FKPoint.XYZ＋O；旋轉軸依中心／方向／M/S 建立旋轉；WorkPiece 自身變換＝T(O)，Laser 自身為單位變換 | 工件鏈上的軸運動變換取反；不能將線性軸公式套用至所有元件 |
| 3. 機構鏈疊加 | T元件最終＝T祖先1 × … × T祖先n × T元件自身 | 得到作用於原始模型幾何的變換，不是單一位置點；乘法順序不可任意交換 |

SolveFK 補償考慮旋轉角度、樞軸幾何及刀長；依構型使用不同公式：
- Spindle：刀頭側補償，內部位置補償公式不使用 P / O。
- Table：加工點相對轉台樞軸的位置影響補償，使用 P / O。
- Mix：合併刀頭側與工作台側補償，工作台側使用 P / O。

**O 必須繼續傳入影格流程**：它在 SolveFK 中影響補償幾何，不等於已直接加到 FKPoint。影格階段還要用於線性軸平移及工件放置；這是不同用途，不是重算一次 RTCP 或可任意刪除的重複加值。工件最終變換為「工件祖先鏈 × T(O)」，偏移也會受祖先旋轉影響。

### 5. 誰使用位置補償與影格？

| 使用者 | 呼叫鏈／輸入 |
| --- | --- |
| 整條路徑模擬 | SimuEditor.CalSimulationResult → BuildSimuData → FiveAxisSimuPathBuilder.BuildFrameTransMap → GetWholePathIKPnt → BuildPathFKList → ConvertIKPntToFKPnt → BuildFKPostPntFiveAxis → SolveFK；再 BuildFrame → FKToFrameTranfResult |
| 刀向編輯單點預覽 | ToolVectorAction.RefreshSimuResult → CalSimuTranfResult → BuildSinglePointFrameTransform；位置取 selected point，角度取修改點 Mod M/S 或 UI M/S（degree 轉 rad） |
| 鎖軸單點預覽 | LockABAction.GetSimulationData → BuildSinglePointFrameTransform；傳入 MainPath 起點位置與 Mod M/S |

- 單點 Builder 將分別傳入的位置與姿態組成 G54 PostPoint，走相同 BuildFKPostPntFiveAxis 與 FKToFrameTranfResult；**位置補償不只由 SimuEditor 使用，也服務 CAM 編輯預覽**。
- 目前 SolveFK 的直接呼叫集中於 BuildFKPostPntFiveAxis；新增用途應先確認要的是補償向量、中間 FK 點，還是完整元件影格。
- MachineChainBuilder 從 RootNode 建立 ChainListMap（祖先清單，不含自己）及 WorkPieceChain（承載工件的祖先集合）。SimulationHelper 依鏈順序累乘自身變換。
- 最終 FrameTransformMap 同時供 MachineRender 顯示與 CollisionHelper 碰撞使用；SimuEditor.SetWorkPieceTransform 將 WorkPiece 影格套用至零件及加工路徑。

### 6. 維護入口

- 核心姿態／補償：MyCAM/Post/PostSolver.cs、FiveAxisSolver.cs。
- 加工姿態／局部 AB：MyCAM/PathCache/ContourCache.cs、StdPatternCacheBase.cs、各圖形 Cache；MyCAM/Helper/CAM/ToolVecHelper.cs。
- UI 轉換與單點預覽：MyCAM/Editor/Action/CAM/ToolVectorAction.cs、LockABAction.cs。
- FK 點／完整影格：MyCAM/Helper/Simulation/FiveAxisSimuPathBuilder.cs、SimulationHelper.cs、PerFrameTransformBuilder.cs。
- 機構鏈／影格使用者：MyCAM/Data/Machine/MachineChainBuilder.cs；MyCAM/Editor/SimuEditor.cs、Editor/Renderer/MachineRender.cs；MyCAM/Helper/Simulation/CollisionHelper.cs。

## MachineData：資料層次、生命週期與使用者

以下為目前實作的導覽，修改前須核對程式碼。MachineData 不只是運動學參數：它同時提供產品／構型、旋轉軸能力、幾何方向、模擬拓樸與資源。**「取得 MachineData」不等於「依構型分流」**；應區分能力／策略決策、數值讀取與單純轉交。FK / IK 的公式與影格責任詳見前一章。

### 1. 資料層次與術語

| 層次 | 型別／資訊 | 意義與用途 |
| --- | --- | --- |
| 產品 | IMachineDataBase、Product；FiveAxisMachineData / ArticRobotMachineData | 共用資料與產品專屬實作的界線；工廠選擇 Builder、Renderer 及資源 |
| 五軸構型 | FiveAxisType：Spindle / Table / Mix | 刀頭／工作台的旋轉軸角色、加工策略、預設機構樹 |
| 旋轉軸能力 | MixTypeVariant：Mix / MasterOnly / SlaveOnly | 雙軸或單軸；限制 UI 與 NC 輸出軸集合 |
| 軸定義 | MasterRotaryAxis / SlaveRotaryAxis、RotateDir、RotateCenter、軸範圍 | M/S 對應實體軸、姿態求解、位置補償與影格 |
| 加工幾何方向 | TubeDir、ToolDir、RotaryAxisConfig | 管件／圖形對齊、安全面方向、刀向與操作軸名稱；彼此不是同一概念 |
| 模擬設定／拓樸 | SimulationOffset、FullMachineSimulation、RootNode、ChainListMap、WorkPieceChain | 工件放置、選擇與建立機構鏈 |
| 模擬資源／對象 | ToolLength、MachineMeshes、MeshFileNameMap、ComponentTypes、ToolChainLeaf / WorkPieceChainLeaf | 刀具顯示、元件載入／索引、碰撞對象組織 |

- SingleSpindle / SingleTable 是 FiveAxisType 的**檔案格式代碼**；執行期單旋轉軸使用 MixTypeMachineData＋MixTypeVariant，不要僅以檔案代碼判斷功能。
- M/S 是機台旋轉軸角色，不固定等於 A/B/C；MasterRotaryAxis / SlaveRotaryAxis 的 X/Y/Z 定義再映射為 A/B/C 軸名。
- RotaryAxisConfig 是 UI 使用的小型資料：MasterName、SlaveName、RotaryAxisName、RotaryAxisType、Variant / IsSingleAxis。FiveAxisMachineData 目前以 Spindle 的 Master、Table / Mix 的 Slave 作為可翻轉旋轉軸。
- TubeDir 未指定時，InitTubeDir 以該旋轉軸作預設；這不代表所有幾何方向都必須使用 TubeDir。

### 2. 載入、持有與機構鏈建立

主要流向：`機台設定 XML → MachineDataXMLHelper / MachineDataDTO → StartupForm → DataManager.MachineData → BuildChain → MachineChainBuilder → 應用層取得資料`。

- MachineDataXMLHelper 處理構型代碼、單軸變體、方向、刀長、SimulationOffset、FullMachineSimulation 及機構樹的讀寫；MachineDataDTO 是機台設定轉換資料，不等同加工專案 DTO。
- DataManager 持有 MachineData；設定 MachineData 時重建機構鏈，尚未設定時使用預設機台資料並建立鏈。MachineMeshes 經 DataManager 存取，但實際存於 MachineData，是不序列化的執行期資源。
- DataGettingHelper.GetMachineData **只取得 FiveAxisMachineData**，其他產品型別回傳失敗。使用此入口的 CAM Action / Cache 不能因此視為跨產品通用。
- 五軸非完整機台模擬會使用預設機構樹；完整模擬則檢查設定樹，目前要求 Base 下有兩條不分岔的刀具／工件鏈，且末端符合指定 leaf。無效時仍改用預設樹；預設樹依 Spindle / Table / Mix 選擇。
- MachineChainBuilder 由 RootNode 建立 ChainListMap（依根到父節點排序的祖先，不含自己）與 WorkPieceChain（工件端祖先集合）。不是每次讀取 MachineData 屬性都重新建鏈，也不能假設修改任意欄位會自動重建所有衍生資料。
- 目前加工專案 DataManagerDTO 不包含 MachineData；儲存加工專案與儲存機台設定是不同流程。

### 3. 應用層：功能限制與加工策略

| 使用者／入口 | 使用資訊 | 實際用途 |
| --- | --- | --- |
| ToolVectorAction.Start → ToolVectorDlg | RotaryAxisConfig | 顯示軸名、依單軸能力限制插補與刀向編輯功能 |
| LockABAction.Start → LockABDlg | RotaryAxisConfig＋FiveAxisType | 限制可鎖定軸、加工方向與翻轉操作；另結合模式與選取狀態 |
| ContourCache.CalMSForEachPoint → SetToolVecByAssignDir | FiveAxisType、RotaryAxisConfig | 將上層「加工方向」需求轉成每條輪廓的固定 M/S；Table / Mix 才執行起點相位對齊 |
| ToolVecHelper.FlipRotaryAxis | FiveAxisType、ToolDir、Master/SlaveRotateDir | 決定翻轉 Master 或 Slave，以及另一軸的翻轉規則；奇異判斷另交 PostSolver |
| ToolVecHelper.SolveIKAndFilterSingularity | FiveAxisType | 求解後選擇處理 Master 或 Slave 的奇異點角度，屬求解器外圍策略 |

#### 刀向／鎖軸 UI 的能力限制

- ToolVectorDlg 的單軸機台只提供 Normal / VectorInterpolation，停用不存在的軸、局部 AB 傾角輸入、旋轉軸翻轉與不適用圖例；這是目前 UI 能力策略，不是所有插補模式都有通用單軸實作。
- LockABDlg 的「旋轉方向調整」（翻轉按鈕）與「加工方向」（指定方位）是不同區域。一般模式會停用外層鎖軸區；鎖軸模式下，以以下條件控制子區域：

| 區域 | 啟用條件 |
| --- | --- |
| Master 設定 | 勾選鎖 Master，且不是 SlaveOnly |
| Slave 設定 | 勾選鎖 Slave，且不是 MasterOnly |
| 旋轉方向調整 | Slave 區域可啟用，且只選一條路徑 |
| 加工方向 | Slave 區域可啟用，且不是 Spindle；不要求單選 |

Spindle 顯示不支援加工方向的提示，但不因此停用所有旋轉軸調整。這是目前「工件相位對齊」功能的限制，不是機台能否從不同方向加工的通用結論。

#### 加工方向：上層需求與 Cache 的轉換

使用者指定「這條輪廓的起始加工位置，要由工件旋轉軸轉到哪個方位」，而不是直接填相同的旋轉角度；不同輪廓的起點不同，選相同方位仍可能需要不同角度。此處不是路徑順／逆向或引刀方向。

`LockABDlg.MachiningDirChanged → LockABAction.OnMachiningDirChanged → 重設各路徑 SpecialToolVecData、鎖定兩軸並寫 MachiningDir → Mode＝MachiningDirection → ContourCache 重建`。

- Cache 先由指定旋轉軸方向求一組 M/S，再計算「目標方向相位－MainPath 起點相位」覆寫 Slave，保留求得的 Master；以 CraftData.InitSlave_rad 選最近的等價角度，再將固定 M/S 套用整條輪廓。
- 方向名稱依軸所在平面映射，例如繞 Z 時為 X+ / Y+ / X− / Y−；相位正負依 RotaryAxisHelper 的左手旋轉慣例。
- ToolVec 只描述刀向，不能總是唯一決定工件旋轉相位；起點位置是額外加工策略條件，不是 RTCP 補償。
- 目前取 **MainPath 第一點相對 G54 原點**，不是 LeadIn 起點、輪廓中心、機台實體旋轉中心，也不保證整條輪廓每點都面向該方位。Table / Mix 執行此策略，Spindle 不套用相同工件旋轉邏輯。

### 4. 幾何用途：方向參數不等於構型策略

| 使用者 | 使用資訊 | 目的 |
| --- | --- | --- |
| AddPathAction.GetTargetAlignAxis | TubeDir | 取得路徑建立流程的目標對齊軸 |
| PlaceRingShapeAction | TubeDir | 旋轉體置放的對齊軸 |
| TraverseHelper.GetSafeBoundaryAxis | TubeDir | 安全面軸向；實際投影／路徑函式使用上游傳入的 axis |
| TraverseHelper.GetBoundaryReferenceDirection | Master/SlaveRotateDir | 無有效投影方向時的候選，不求旋轉角或 FK / IK |

- 「幾何方向退化」指點位於圓柱軸線或球心，徑向向量接近零、無法決定唯一投影方向；不是運動學奇異點。
- TryProjectTraversePoint 先用位置求徑向，再使用 ToolVec（圓柱須移除軸向分量）；仍為零才選參考方向。GetBoundaryReferenceDirection 依序嘗試安全面軸向、機台 Master/Slave 方向，投影到垂直軸向的平面，最後用全域 X/Y/Z 備用方向。
- TubeDir 決定軸向，不能單靠同一軸向決定圓柱側面的徑向；在此呼叫中安全面軸向候選本身會被消掉。借用機台軸方向只是退路，不表示唯一正確方向或經碰撞驗證的安全方向。
- RotaryAxisHelper 主要接收上游參數做軸名／方向／相位映射，不自行取得 MachineData。

### 5. NC、模擬與顯示的非求解用途

| 使用者 | 使用資訊 | 目的 |
| --- | --- | --- |
| NCWriter → AxisConfig → NCWriterHelper | Master/SlaveRotaryAxis、MixTypeVariant | M/S 映射 A/B/C；省略單軸機台不存在的軸；雙軸依 RotaryAxis 列舉順序輸出 |
| SimuPathBuilderFactory、PerFrameTransformBuilderFactory、MachineRenderFactory | MachineData 型別／Product | 選產品實作；不同工廠的支援程度不可視為一致 |
| StartupForm.GetMachineMeshFolderName → ReadMachineMeshHelper | Product / FiveAxisType、MeshFileNameMap | 選機台 Mesh 資料夾並載入各元件 STL |
| FiveAxisMachineRender / ArticRobotMachineRender | ToolDir＋ToolLength / FlangeFrame | 建立刀具／雷射幾何或法蘭座標框 |
| CollisionHelper | ComponentTypes、ToolChainLeaf、WorkPieceChainLeaf、ChainListMap | 元件索引、辨識刀具側／工件側並組織碰撞對象 |
| SimuEditor、ToolVectorAction | SimulationOffset | 額外建立 G54 座標圖示 |
| LockABAction.GetSimulationData | 轉交 MachineData 至單點 Builder | 間接使用 SimulationOffset 做補償與影格；沒有另建 G54 座標圖示 |

- AxisConfig 是 NC 使用的縮小資料，不等同 UI 的 RotaryAxisConfig。NC 自訂 Header / Tail 等主要來自 CustomizedPostInfo，校正段主要看校正資料，不應全部歸為機台構型決策。
- 目前 ArticRobot 有影格 Builder 與 Renderer 分支，但 SimuPathBuilderFactory 的機器人路徑建立仍不支援；不能由資料型別存在推論整條模擬已完成。
- 機構影格與 SimulationOffset 的運算責任沿用前一章；顯示座標圖示、間接參與單點預覽與直接求補償是不同使用目的。

### 6. 維護界線與程式碼入口

- 目前能力策略分散於 UI、Cache、Helper 與工廠，並非由單一策略層統一決策。新增構型時須一起檢查 UI 可用性、加工方向、翻轉／奇異處理、NC 輸出軸與模擬支援。
- 優先辨識呼叫者需要的是產品選型、軸能力、單一方向參數或完整機構拓樸；不要把空值防護、型別守門或單純轉交求解器當成構型策略。
- 模型／能力／拓樸：MyCAM/Data/Machine/ 下的 MachineData.cs、MachineDefine.cs、ComponentInfo.cs、MachineChainBuilder.cs、MachineDataFactory.cs。
- 載入／持有：MyCAM/StartupForm.cs、Data/DataManager.cs、Data/DataGettingHelper.cs、FileManager/MachineDataDTO.cs、Helper/FileIO/MachineDataXMLHelper.cs。
- 編輯／策略：MyCAM/Editor/Action/CAM/ 下的 ToolVectorAction.cs、LockABAction.cs；Editor/Dialog/ 下的 ToolVectorDlg.cs、LockABDlg.cs；PathCache/ContourCache.cs、Helper/CAM/ToolVecHelper.cs。
- 幾何：MyCAM/Editor/Action/CAM/AddPathAction.cs、Editor/Action/CAD/PlaceRingShapeAction.cs、Helper/CAM/TraverseHelper.cs、RotaryAxisHelper.cs。
- NC／模擬：MyCAM/Post/ 下的 AxisConfig.cs、NCWriter.cs、NCWriterHelper.cs；Helper/Simulation/ 下的工廠、ReadMachineMeshHelper.cs、CollisionHelper.cs；Editor/Renderer/MachineRender.cs。
