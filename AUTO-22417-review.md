## 施工模組與權責
### 模組總覽

| 模組 | 核心權責 |
| --- | --- |
| M1 機台定義、拓樸與機台持久化 | 描述有哪些外部軸、零位幾何及工件側依附關係，載入並驗證設定 |
| M2 工藝參數、點資料與後處理轉裝 | 保存每條路徑的指派角度，讓角度隨加工點、補點及空移點傳遞 |
| M3 共用變位轉換與五軸求解 | 統一位置／方向轉換，銜接姿態 IK、刀向正解與 RTCP 補償 |
| M4 連續分組、組端點與 IK 連續性 | 定義共同分組邊界，保留組端點，重設每組的 IK 起始種子 |
| M5 空移與進退刀顯示 | 只連接同組路徑，管理各組 Entry／Exit 圖元及失效清理 |
| M6 模擬點列、FK 與機構影格 | 逐組建立模擬資料，套用變位與補償，再建立元件影格 |
| M7 NC 與客製後處理 | 逐組輸出變位區段，轉換 CAM 寫出的路徑位置，維持控制器契約邊界 |
| M8 編程介面與編輯生命週期 | 單／雙軸與多選編輯、預覽、取消還原，以及確認後串接最佳化 |
| M9 驗證、範例與施工文件 | 提供各責任層的可重複驗證及交付依據 |

## M1 機台定義、拓樸與機台持久化

**權責：** 定義機台能力與外部軸零位資料，建立可供求解／模擬使用的軸與拓樸；不保存各加工路徑的編程角度。

**Review：OK。**

### 新增定義與位置

| 定義 | 所在檔案／歸屬 | 意義 |
| --- | --- | --- |
| `ExternalAxisType` | [MachineDefine.cs](MyCAM/Data/Machine/MachineDefine.cs)，`MyCAM.Data` 命名空間 | 新增列舉：None＝0 無變位機、Single＝1 單轉台、Dual＝2 雙轉台。 |
| `MachineComponentType.Ext1`、`MachineComponentType.Ext2` | 同檔既有 `MachineComponentType` 列舉 | 新增模擬元件識別值 **30／31**，保留 18～29，其他既有成員數值不變。 |
| `MachineParamName.EXTERNAL_AXIS_TYPE` | 同檔既有 `MachineParamName` 類別 | 新增字串常數 `ExternalAxis_Type`，作為機台 XML 的 Pr 節點 ID。 |
| `ExternalAxisMachineData` | [MachineData.cs](MyCAM/Data/Machine/MachineData.cs) | 新增機台設定類別，包含外部軸型別、幾何、範圍及初始化後的旋轉軸。 |
| `FiveAxisMachineData.ExternalAxes` | 同檔既有五軸機台基底類別 | 新增設定持有屬性，預設建立 `ExternalAxisMachineData`，Type 預設 None。 |
| `ExternalAxisMachineDataDTO` | [MachineDataDTO.cs](MyCAM/FileManager/MachineDataDTO.cs) | 新增轉換資料類別。 |
| `MachineDataDTO.ExternalAxes` | 同檔既有 DTO 基底類別 | 新增外部軸設定欄位，銜接各五軸構型的 DTO 往返。 |

`RotaryAxis`、`RotaryDirection`、`RotateAxisDef` 及 `MachineTreeNode` 均沿用既有定義。

**ExternalAxisMachineData 的資料欄位：**

| 成員 | 型別／單位 | 意義與預設 |
| --- | --- | --- |
| `Type` | ExternalAxisType | 預設 None；決定有效軸數。 |
| `Ext1RotaryAxis`、`Ext2RotaryAxis` | RotaryAxis | 名義 X／Y／Z 軸；預設 None，但啟用的軸不能為 None。 |
| `MCSToExt1Vec` | gp_Vec，mm | MCS 零位原點到 Ext1 中心的向量；預設零向量。 |
| `Ext1ToExt2Vec` | gp_Vec，mm | Ext1 到 Ext2 零位中心的向量，分量仍在 MCS 零位共用基底；Ext2 中心為兩向量相加。預設零向量。 |
| `Ext1TiltedVec_deg`、`Ext2TiltedVec_deg` | gp_XYZ，degree | 各軸三方向傾斜量，預設零；只保存，**不參與實際軸方向計算**。 |
| `Ext1RotaryDirection`、`Ext2RotaryDirection` | RotaryDirection | 旋轉慣例中繼資料，預設 None 且合法；本版本實際運算**固定左手**。 |
| `Ext1Start_deg`、`Ext1End_deg`、`Ext2Start_deg`、`Ext2End_deg` | double，degree | 外部軸範圍，預設零；驗證 Start ≤ End。 |
| `Ext1Axis`、`Ext2Axis` | RotateAxisDef，private setter | `InitRotateAxes` 產生的執行期軸，初始化前為 null；Single 不建立 Ext2Axis，None 不建立外部軸。 |

DTO 的同名欄位對應上述持久化資料，向量用 `gp_Vec_DTO`、傾斜量用 `gp_XYZDTO`；`ToMachineData` 重建模型，實際軸由後續初始化建立。機台參數改動後仍須顯式初始化並重建機構鏈。

[MachineDataXMLHelper.cs](MyCAM/Helper/FileIO/MachineDataXMLHelper.cs) 負責具名 XML 讀寫，舊檔缺少外部軸設定時視為 None；[StartupForm.cs](MyCAM/StartupForm.cs) 接收 `errorParameterId` 回報具名欄位錯誤。

### 元件清單與 Mesh 對應

實作位於 `FiveAxisMachineData.ComponentTypes`／`MeshFileNameMap`。Ext1 是第一個變位機關節；雙轉台的 Ext2 是第二個關節，依附於 Ext1。

| 元件 | MeshFileNameMap 的值 | 由載入器組成的檔名 | 本次異動／啟用條件 |
| --- | --- | --- | --- |
| Base | `Base` | Base.stl | 既有對應，不變。 |
| XAxis | `X` | X.stl | 既有對應，不變。 |
| YAxis | `Y` | Y.stl | 既有對應，不變。 |
| ZAxis | `Z` | Z.stl | 既有對應，不變。 |
| Master | `Master` | Master.stl | 既有對應，不變。 |
| Slave | `Slave` | Slave.stl | 既有對應，不變。 |
| Ext1＝30 | `Ext1` | **Ext1.stl** | **新增**；Single／Dual 才加入對應。 |
| Ext2＝31 | `Ext2` | **Ext2.stl** | **新增**；只有 Dual 加入對應。 |
| Laser／WorkPiece | 無此表對應 | 不由此表指定 STL | 沿用原流程，不新增固定檔名。 |

`ComponentTypes` 原有順序為 `XAxis, YAxis, ZAxis, Master, Slave, Laser, WorkPiece`，共 7 個。
Single 追加 Ext1，共 8 個；Dual 追加 Ext1、Ext2，共 9 個。
分別由新增的 `s_SingleExternalComponentTypes`／`s_DualExternalComponentTypes` 提供。

Mesh 載入沿用 [ReadMachineMeshHelper.LoadMahineMesh](MyCAM/Helper/Simulation/ReadMachineMeshHelper.cs)：
組成 `Path.Combine(機台 Mesh 資料夾, 對應值 + ".stl")`，結果存於 `MachineMeshes.Meshes[元件型別]`。
資料夾仍依 Spindle／Mix 等既有構型選擇，沒有另建變位機專用資料夾或 XML 檔名參數。

### 預設樹 (當前認知下的最快解)

`BuildDefaultTree` 先依構型建立 `SimuData.TreeData` 的原始預設樹，再由新增的 `InsertExternalAxes` 找到 WorkPiece，將原父節點到 WorkPiece 的連結替換為外部軸鏈。**不新增第三條 Base 分支，也不改寫刀具分支。**

| 構型／變位機 | 原工件分支 | 插入後的工件分支 |
| --- | --- | --- |
| 無變位機（None） | 原構型的工件分支 | 不插入，維持原樹。 |
| Spindle＋Single | Base → WorkPiece | **Base → Ext1 → WorkPiece** |
| Spindle＋Dual | Base → WorkPiece | **Base → Ext1 → Ext2 → WorkPiece** |
| Mix＋Single | Base → Slave → WorkPiece | **Base → Slave → Ext1 → WorkPiece** |

Mix 的外部軸位於既有工件側 Slave 之後；雙轉台固定由 Ext1 承載 Ext2，再承載 WorkPiece。
Table＋外部軸、Mix＋Dual 不在支援範圍，先由 `ValidateFor` 拒絕。

插入完成後沿用 [MachineChainBuilder.Build](MyCAM/Data/Machine/MachineChainBuilder.cs) 建立 `ChainListMap` 與 `WorkPieceChain`。
Spindle＋Dual 的 WorkPiece 祖先為 Base、Ext1、Ext2；Mix＋Single 為 Base、Slave、Ext1，讓後續 WorkPiece 影格包含對應外部軸運動。

### 完整樹位置驗證 (後續需考慮手臂相容性)

`NeedBuildDefaultTree` 回傳值表示「是否需要改用預設樹」。`FullMachineSimulation == false` 時直接使用預設樹；為 true 時才檢查設定提供的 RootNode。

既有基本檢查先確認根為 Base，Base 恰有兩條不再分岔的鏈，末端分別為 Laser 與 WorkPiece，分支排列順序不拘。本議題新增的 `HasValidExternalAxisPlacement` 再檢查**外部軸的數量、側別及相鄰順序**，不是檢查旋轉中心的幾何位置。

| 情況 | 外部軸的具體要求 |
| --- | --- |
| 刀具分支 | 不得出現 Ext1 或 Ext2；變位機只能在工件側。 |
| ExternalAxisType.None | 兩條分支都不得出現 Ext1／Ext2。 |
| Single 的工件分支 | 恰好一個 Ext1、零個 Ext2，末端必須是 **Ext1 → WorkPiece**。 |
| Dual 的工件分支 | Ext1／Ext2 各恰好一個，末端必須是 **Ext1 → Ext2 → WorkPiece**。 |
| Mix 且啟用變位機 | 工件分支還必須包含既有 Slave。 |

因此會拒絕外部軸缺漏、重複、放錯側、Ext1／Ext2 顛倒，或在外部軸鏈與 WorkPiece 之間插入其他元件。**Mix 只額外確認 Slave 在工件鏈內，未要求 Slave 必須緊鄰 Ext1**；兩者相鄰是預設樹的排列。

**驗證不通過時：**
`NeedBuildDefaultTree` 回傳 true，既有 `MachineChainBuilder.Build` 將整個 RootNode 換成 `BuildDefaultTree()` 的結果，再重建祖先鏈，不修補原完整樹。合法則保留原樹。
外部軸參數本身非法時，前置 `ValidateFor` 會拋錯，不走這個拓樸回退機制。

## M2 工藝參數、點資料與後處理轉裝

**權責：** 將「路徑指派的外部角度」當作加工資料傳遞，不在資料複製或轉裝時改寫 XYZ，也不自行重求姿態。

**Review：OK。**

| 施工對象 | 施工內容 |
| --- | --- |
| [CraftData.cs](MyCAM/Data/Craft/CraftData.cs)、[DataManagerDTO.cs](MyCAM/FileManager/DataManagerDTO.cs) | 新增 degree 單位的 `ExternalAxisPos` 值型資料、工藝屬性與 DTO；拒絕非有限值、保留多圈角度，異動觸發 `CAMFactorChanged`，支援舊加工專案缺節點時預設零。 |
| [PointData.cs](MyCAM/Data/PointData.cs)、[PostData.cs](MyCAM/Data/PostData.cs) | CAMPoint、相關點介面、五軸 PostPoint 與 Clone 攜帶角度；不擴充 ArticRobotPostPoint。 |
| [ContourCache.cs](MyCAM/PathCache/ContourCache.cs)、[StdPatternCacheBase.cs](MyCAM/PathCache/StdPatternCacheBase.cs) | 在加工點／參考點求 IK 前指定角度，並傳至標準圖元加工點；工藝異動沿既有 dirty 流程重建。 |
| [LeadHelper.cs](MyCAM/Helper/CAM/LeadHelper.cs)、[OverCutHelper.cs](MyCAM/Helper/CAM/OverCutHelper.cs)、[MicroJointHelper.cs](MyCAM/Helper/CAM/MicroJointHelper.cs)、[TraverseHelper.cs](MyCAM/Helper/CAM/TraverseHelper.cs) | 加工補點沿用參考加工點角度；暫時空移 CAMPoint 及零距離 Clone 保留來源角度。 |
| [ContourPostHelper.cs](MyCAM/Post/ContourPostHelper.cs)、[StdPatternPostHelper.cs](MyCAM/Post/StdPatternPostHelper.cs)、[PostTraverseBuilder.cs](MyCAM/Post/PostTraverseBuilder.cs) | 加工點／標準圖元參考點轉裝時保留角度；Entry／Exit、抬刀／下刀、蛙跳及安全面 PostPoint 使用相應端點的角度。 |

**注意：**
- 組內空移維持固定外部角度，不插補外部軸。
- 暫時空移 CAMPoint 在此階段只帶入變位機角度；空移 M/S 仍由後處理沿用或插補。模擬新建點的傳遞接點歸 M6，既有姿態編輯預覽接點歸 M8。

## M3 共用變位轉換與五軸求解

**權責：** 統一處理外部軸幾何影響，並與既有五軸姿態求解、RTCP 位置補償分開。

| 施工對象 | 施工內容 |
| --- | --- |
| [PostSolver.cs](MyCAM/Post/PostSolver.cs) | 新增各軸／組合變換、位置／方向／向量正反轉換與 `GetExternalDisplacement`。`SolveIK` 先旋轉刀向；帶外部角度的 `SolveToolVec` 反轉變位旋轉；`SolveFK` 先取得已變位位置，`SolveFKAtPositionedPoint` 則明確接收已變位點，兩者均只回傳五軸 RTCP 補償。 |
| [ContourCache.cs](MyCAM/PathCache/ContourCache.cs)、[StdPatternCacheBase.cs](MyCAM/PathCache/StdPatternCacheBase.cs)（求解接點） | 將點上的外部角度傳入初始姿態及刀向回算流程；不在 Cache 預先變位幾何。 |
| [ToolVecHelper.cs](MyCAM/Helper/CAM/ToolVecHelper.cs) | 指定／鎖軸、局部 AB、傾角與插補相關求解傳入角度；局部幾何仍以原工件座標處理，翻轉與奇異判斷保持既有五軸求解座標。 |

**責任界線：** 外部正角度採工件側左手慣例，即 OCC 負 radian；`RotaryDirection` 不再額外改變外部旋轉正負。位置轉換為 `Pₑ＝T_ext(P＋O)－O`，組合變換為 `T_Ext1 × T_Ext2`；方向不帶平移。外部角度為 degree，M/S 維持 radian。變位位移不混入 RTCP 補償回傳值。

## M4 連續分組、組端點與 IK 連續性

**權責：** 提供顯示、NC、模擬及 IK 最佳化共同使用的加工分組邊界，而非讓各消費端各自判斷角度是否相同。

| 施工對象 | 施工內容 |
| --- | --- |
| [CacheHelper.cs](MyCAM/PathCache/CacheHelper.cs) | 新增 `ProcessPathGroup`、`GetProcessPathGroups` 及分組版 `OptimizeIKContinuity`；過濾不加工路徑，保留原始路徑索引，依有效軸精確值連續分組。每組 M/S 種子從零開始，組內承接前一加工終點的修改後角度；起終點沿用既有 CacheHelper 規則。 |
| [SimuData.cs](MyCAM/Data/Simulation/SimuData.cs) | 新增 `ProcessGroupPostData`，保存組資訊、組內 PostData 及 Clone 後的完整首末 CAMPoint；模擬輸入增加 `ProcessGroups`。 |
| [NCWriter.cs](MyCAM/Post/NCWriter.cs)（後處理組裝） | `ConvertPostDataListSuccess` 逐組重設前一路徑狀態，產生組內後處理資料與組端點；保留原扁平輸出介面，標準圖元模擬仍取得完整加工點。 |
| [CAMEditor.cs](MyCAM/Editor/CAMEditor.cs)、[SimuEditor.cs](MyCAM/Editor/SimuEditor.cs)（協調接點） | 前者將既有全域 IK 最佳化導向共用分組；後者取得並傳送組資料與完整組末點到模擬輸入。 |

**責任界線：** 不排序、不合併不相鄰的同角度組、不做近似比較或週期正規化，0°／360° 是不同位置。加工起點優先 LeadIn；終點優先 LeadOut，其次 OverCut，否則 MainPath。各組均有 Entry／Exit，不能只留不帶 ToolVec 的 PostPoint 來重建組末退刀。

## M5 空移與進退刀顯示

**權責：** 根據共同分組及既有空移幾何建立 AIS，並維護圖元生命週期；不是變位求解器，也不是模擬細插器。

| 施工對象 | 施工內容 |
| --- | --- |
| [TraverseRenderer.cs](MyCAM/Editor/Renderer/TraverseRenderer.cs) | 使用同組前一路徑對應，只建立組內 Traverse；每組 Entry／Exit 使用含首／末路徑 ID 的獨立鍵；重建時清除舊組端點及刪除／停用路徑圖元，局部更新略過不加工路徑，已刪除 ID 則全量失效重建。 |

**責任界線：** AIS 在原工件座標建立，預覽／播放再套用 WorkPiece 影格；不在 Renderer 重算變位或 RTCP。`TraverseHelper` 的幾何計算仍在原工件座標，該檔本次主要是 M2 的角度傳遞；安全面設定預覽不要求與正式組內空移具有相同相鄰關係。

## M6 模擬點列、FK 與機構影格

**權責：** 將逐組後處理資料轉成取樣點、FK 中間點及各元件影格，提供播放、單點預覽與既有碰撞流程使用。

| 施工對象 | 施工內容 |
| --- | --- |
| [FiveAxisSimuPathBuilder.cs](MyCAM/Helper/Simulation/FiveAxisSimuPathBuilder.cs) | `GetGroupedPathIKPnt` 對每組各自建立 Entry／加工／Exit；組內細插、新建退刀點及單點預覽保留外部角度。`BuildFKPostPntFiveAxis` 使用共用變位位置與 RTCP API，建立 `Pₑ＋Δtcp`，只套用一次變位；保留舊扁平輸入相容入口。 |
| [PerFrameTransformBuilder.cs](MyCAM/Helper/Simulation/PerFrameTransformBuilder.cs) | `FiveAxisPerFrameTransformBuilder` 使用共用 API 建立 Ext1／Ext2 自身變換；線性軸仍加 SimulationOffset，WorkPiece 自身仍為該 offset 的平移，再交由既有祖先鏈組合。 |
| [SimulationHelper.cs](MyCAM/Helper/Simulation/SimulationHelper.cs) | 修正既有 `GetPathStartEndIndex` 累積偏移：統一以「下一可用索引」計數，包含式末索引為累積值減一；不新增虛構影格或修改播放端掩蓋偏移。 |
| [SimuEditor.cs](MyCAM/Editor/SimuEditor.cs)（模擬協調） | 將 M4 組資料送進 Builder；播放／碰撞仍使用同一份最終元件影格。 |

**責任界線：** 原工件點列 → 組內空移細插及 M/S 插補 → 變位與五軸 RTCP → 元件自身運動 → 祖先鏈組合。各組僅依序串接，不在組間插補變位影格；碰撞也不保證組間掃掠安全。FK 新建點仍不是完整加工 PostPoint 的等價替代物，不能推論已複製傾角／微接點標記。

## M7 NC 與客製後處理

**權責：** 在輸出邊界套用變位位置，安排每組變位區段及進退刀；控制器狀態切換與機台專用動作不由 CAM 通用流程寫死。

| 施工對象 | 施工內容 |
| --- | --- |
| [CustPost.cs](CommonData/CustPost.cs)、[TxtToCustPostTranslator.cs](FileTranslator/TxtToCustPostTranslator.cs) | 新增可選 `ExternalAxisHeader` 區段與資料接線，不納入舊客製檔必要區段；保留缺漏 null、明確空字串及客製內容的差異。 |
| [NCWriter.cs](MyCAM/Post/NCWriter.cs)（NC 輸出） | 使用 M4 分組，每組一次輸出變位 block，包含第一組與零角度組；順序為上一組 Exit → 本組變位 → Entry。`TransformNCPoint`／`TransformNCTraverse` 使用共用位置轉換並 Clone，轉換輪廓與 CAM 空移座標，不使用模擬 FK、不重算 M/S、不累積修改共用點。 |
| [NCWriter.cs](MyCAM/Post/NCWriter.cs)（既有缺陷修正） | `WriteExitSection` 以實際末加工資料是否存在判斷是否退刀，修正全不加工輸入的多餘 Exit／空參考；未擴大重構 counter 或路徑編號。 |

**目前輸出契約：** 每個實際加工組固定先輸出 `// External axis move`。區段缺漏時，依有效軸名義 X／Y／Z 映射 A／B／C，軸名與 degree 角度成對排序，同列輸出且以分號結尾；明確留空僅保留註解；有客製內容則替換 `ExtAxisPos1`／`ExtAxisPos2`，不自動追加預設指令或分號。無變位機／無有效加工路徑不建立變位 block。

**責任界線：** 標準圖元只轉換 CAM 寫出的空移，參考點與 SY_* 巨集契約不變；巨集已有變位能力仍是未驗證假設。預設輸出不包含 RTCP 關閉／恢復、M/S 回位或等待，也不等同實機安全驗證。

## M8 編程介面與編輯生命週期

**權責：** 提供路徑角度編輯與預覽，管理暫存、提交、取消和顯示清理；求解與影格計算委由 M3／M6。

| 施工對象 | 施工內容 |
| --- | --- |
| [ExternalAxisAction.cs](MyCAM/Editor/Action/CAM/ExternalAxisAction.cs) | 新增 `ExternalAxisEditData`／`ExternalAxisAction`；固定選取範圍、保存原值、暫時套用角度並用加工起點預覽；未確認 End 還原，確認後先結束編輯，再發出最佳化通知。 |
| [ExternalAxisDlg.cs](MyCAM/Editor/Dialog/ExternalAxisDlg.cs)、[ExternalAxisDlg.Designer.cs](MyCAM/Editor/Dialog/ExternalAxisDlg.Designer.cs) | 新增單／雙軸、多選混合值、預覽路徑及預覽／確認／取消介面；依目前文化解析有限數值，留白保留各路徑開啟前原值，單軸不清除非有效 Ext2。 |
| [IEditorAction.cs](MyCAM/Editor/Action/IEditorAction.cs)、[CAMEditor.cs](MyCAM/Editor/CAMEditor.cs) | 新增 Action 類型與 `SetExternalAxisPos`；接上選取驗證、編輯期間鎖定、WorkPiece 預覽、顯示重建／清理及全域 `DoOptimizeIKContinuity`。 |
| [StartupForm.cs](MyCAM/StartupForm.cs)、[StartupForm.Designer.cs](MyCAM/StartupForm.Designer.cs)、[MyCAM.csproj](MyCAM/MyCAM.csproj) | 加入工具列入口、事件及依機台啟用條件；將新 Action／Dialog 納入專案編譯。 |
| [ToolVectorAction.cs](MyCAM/Editor/Action/CAM/ToolVectorAction.cs)、[LockABAction.cs](MyCAM/Editor/Action/CAM/LockABAction.cs) | 既有姿態編輯的單點預覽補傳外部角度，沿用同一單點 Builder；沒有重寫其編輯生命週期。 |

**責任界線：** 不依外部軸行程裁切或正規化輸入；取消不執行最佳化，不清除既有刀向／鎖軸設定。確認編輯與最佳化為兩個前後串接的獨立流程：最佳化例外向上傳遞，但不撤銷已確認角度。

## M9 驗證、範例與施工文件

**權責：** 依各層契約提供可重複驗證，不將自動測試結果推論為人工介面、控制器或實機已驗收。

| 施工資產 | 歸屬與用途 |
| --- | --- |
| [ExternalAxisMachineData.Tests.ps1](tests/ExternalAxisMachineData.Tests.ps1) | M1：機台 XML／DTO、合法性、舊檔、構型與拓樸。 |
| [ExternalAxisPointData.Tests.cs](tests/ExternalAxisPointData.Tests.cs)／[.ps1](tests/ExternalAxisPointData.Tests.ps1) | M2：資料入口、通知、持久化、Clone、Cache 與各類補點的角度傳遞。 |
| [ExternalAxisKinematics.Tests.cs](tests/ExternalAxisKinematics.Tests.cs)／[.ps1](tests/ExternalAxisKinematics.Tests.ps1) | M3：正反轉換、IK／刀向、offset／樞軸、雙軸順序及補償責任。 |
| [ExternalAxisGrouping.Tests.cs](tests/ExternalAxisGrouping.Tests.cs)／[.ps1](tests/ExternalAxisGrouping.Tests.ps1) | M4／M6：有效軸、連續組、端點、種子及影格索引。 |
| [ExternalAxisSimulation.Tests.cs](tests/ExternalAxisSimulation.Tests.cs)／[.ps1](tests/ExternalAxisSimulation.Tests.ps1) | M5／M6：圖元清理、逐組點列、機構影格、單點一致性及取樣碰撞接線。 |
| [ExternalAxisNC.Tests.cs](tests/ExternalAxisNC.Tests.cs)／[.ps1](tests/ExternalAxisNC.Tests.ps1) | M7：變位區段、預設／空白／客製契約、位置轉換、標準圖元及不加工回歸。 |
| [ExternalAxisProgramming.Tests.cs](tests/ExternalAxisProgramming.Tests.cs)／[.ps1](tests/ExternalAxisProgramming.Tests.ps1) | M8 與整合：多選、取消／確認、預覽、最佳化順序，以及指派角度到模擬／NC 的端到端接線。 |
| [SpindleDual.MachineData.xml](examples/ExternalAxis/SpindleDual.MachineData.xml)、[MachineTree.Mix5Plus1.xml](examples/MachineTree.Mix5Plus1.xml) | 機台設定與機構樹範例；輔助檢視支援構型，不等同實機設定或 Mesh 資源交付。 |

測試沿用 .NET Framework、64 位元 Windows PowerShell 與既有 OCC／FCL，相依已建置產物，未新增測試套件。施工與 M1 編號調整的七套回歸均記錄 **139 項通過**，詳細結果保留於施工紀錄，不在本報告重複列述。

**交付範圍：** 本報告引用的七套測試與兩份 examples XML 一併納入版本控制；IDE 暫存、個人專案設定及建置產物不列入交付。原施工紀錄與架構指引保留既有歷史內容。

## 跨模組資料流與後續 review 邊界

- **編程／加工資料：** M8 指派 → M2 保存與 Cache 重建 → M3 求加工姿態；幾何仍保留原工件座標。局部 AB、加工方向及安全面不因外部角度改變而重定義座標基準。
- **共同分組：** M4 → M5 正式空移顯示、M6 逐組模擬及 M7 逐組 NC；LeadIn／LeadOut 是加工段，不可與組級 Entry／Exit 混用。
- **模擬與輸出分流：** M6 使用變位位置＋RTCP 補償＋完整機構鏈；M7 僅轉換 CAM 輸出位置，五軸 RTCP 仍由控制器承擔，不拿模擬 FK 點寫 NC。
- **沿用而非修改：** `ComponentInfo.cs`、`MachineChainBuilder.cs`、`CollisionHelper.cs`、`MachineRender.cs`、`CustPostWriter.cs`、`NCWriterHelper.cs`、`StdPatternNCWriter.cs` 未列入本次 Git 程式差異；新增元件／區段經現有資料驅動流程使用它們，不應誤列為新增演算法或已完成重構。
- **已修正的既有問題：** M6 影格索引累積偏移、M7 全不加工時的 Exit 判斷；與外部軸新功能分開追溯。
- **未擴大施工／未驗證：** 傾斜軸方向運算、圖元轉換的外部角度保留策略、不加工 counter／索引全面盤點；人工 UI／播放、控制器及標準圖元巨集能力、實機與組間變位掃掠安全，均不能由本章盤點視為完成。
