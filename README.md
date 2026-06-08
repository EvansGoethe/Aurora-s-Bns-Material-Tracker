# 洛洛劍靈材料追蹤器 | Aurora's BnS Material Tracker

A desktop tool for **Blade & Soul TW Server** to track dungeon runs, manage materials, and plan upgrades.  
專為**劍靈台服**設計的桌面工具，協助追蹤副本進度、管理材料倉庫與規劃升級路線。

---

## Features 功能

| | |
|---|---|
| **副本進度追蹤** | 每日 / 每週副本完成次數紀錄，支援 +/- 計數與一鍵重置 |
| **Dungeon Tracker** | Track daily & weekly run counts with +/- controls and one-click reset |
| **材料計算器** | 勾選升級步驟，自動加總所需材料並顯示缺口與補齊副本建議 |
| **Material Calculator** | Select upgrade steps to auto-sum requirements, show shortfalls & recommended dungeons |
| **目標達成預測** | 依遊玩時間預測達成升級目標所需天數 |
| **Goal Predictor** | Estimates days to completion based on daily play time |
| **倉庫管理** | 記錄各材料與金幣持有數量，供計算器即時參照 |
| **Inventory** | Log materials and gold for real-time reference by the calculator |
| **效益分析** | 依市場單價計算各副本金幣/分鐘效益 |
| **Market Analysis** | Gold-per-minute efficiency ranking based on market prices |
| **資料編輯器** | 表單式介面直接編輯副本、材料、升級路線，無需手動改 JSON |
| **Data Editor** | Form-based editor for dungeons, materials, and upgrade paths — no JSON editing needed |
| **截圖辨識** | 匯入背包截圖，自動比對圖示並讀取數量，一鍵套用至倉庫 |
| **Bag Scanner** | Import a bag screenshot, auto-match item icons and read quantities via OCR, then apply to inventory in one click |
| **多語言** | 繁體中文 / 簡體中文 / English 即時切換 |
| **i18n** | Traditional Chinese / Simplified Chinese / English |

---

## Requirements 執行需求

- Windows 10 / 11 (x64)
- [.NET 6.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)（免安裝版需要，安裝器版本會自動提示）

---

## Installation 安裝方式

### 方式一：安裝器（推薦）Installer (Recommended)

至 [Releases](../../releases) 下載 `Setup_Aurora_BnS_Material_Tracker_vX.X.X.exe`，執行後依指示完成安裝。  
Download `Setup_Aurora_BnS_Material_Tracker_vX.X.X.exe` from [Releases](../../releases) and follow the setup wizard.

- 安裝器支援繁體中文 / 簡體中文 / English 介面
- Installer UI available in Traditional Chinese / Simplified Chinese / English
- 自動建立桌面捷徑與開始功能表項目
- Creates desktop shortcut and Start Menu entry automatically
- 提供標準解除安裝程式
- Includes a standard uninstaller

### 方式二：免安裝版 Portable

1. 至 [Releases](../../releases) 下載 `Aurora_BnS_Material_Tracker_vX.X.X_portable.exe`
2. 建立一個資料夾，將 exe 與 `Data/` 資料夾放在同一目錄下
3. 直接執行 exe，無需安裝

---

## Data Files 資料檔案

遊戲資料以 JSON 格式儲存於 exe 旁邊的 `Data/` 資料夾，可透過程式內的**資料編輯**頁面直接修改，不需要手動編輯檔案。

Game data is stored as JSON files in the `Data/` folder next to the exe. Use the built-in **Data Editor** page to modify them without touching the files directly.

| 檔案 | 內容 |
|---|---|
| `dungeons.json` | 副本清單、類型、難度、掉落設定 |
| `materials.json` | 材料清單與分類 |
| `upgrades.json` | 各裝備部位的升級路線與材料需求 |

---

## Save Data 存檔位置

使用者資料（倉庫數量、副本進度、升級目標）儲存於：

```
%AppData%\BnsMaterialTracker\save.json
```

解除安裝程式不會刪除此檔案，重新安裝後資料仍會保留。  
The uninstaller does not remove this file — your data is preserved across reinstalls.

---

## Bag Scanner 截圖辨識

自動從背包截圖讀取材料數量並同步至倉庫，免去手動輸入。

1. 切換至「截圖辨識」頁籤，點擊「📂 匯入截圖」載入背包截圖
2. 在截圖上點擊要追蹤的材料格子，從彈出選單指定對應材料，完成模板登記
3. 重複步驟 2 直到所有材料都登記完畢（模板只需登記一次，之後會自動沿用）
4. 點擊「🔍 掃描截圖」，程式會自動比對圖示位置並 OCR 讀取數量
5. 確認結果無誤後，點擊「✅ 套用至倉庫」將數量寫入倉庫

> **格子大小**：預設 64px，可依遊戲解析度調整（側欄的「＋／－」按鈕）。  
> **全圖搜尋**：若遊戲視窗有移動，勾選「全圖搜尋」讓程式在整張截圖中尋找圖示。

---

Automatically read material quantities from a bag screenshot and sync them to your inventory — no manual entry needed.

1. Go to the **Bag Scanner** tab and click **📂 Import Screenshot** to load a bag screenshot
2. Click on an item cell in the screenshot; assign the matching material from the popup to register a template
3. Repeat step 2 for each material you want to track (templates are saved and reused automatically)
4. Click **🔍 Scan Screenshot** — the app matches icon positions and reads quantities via OCR
5. Review the results, then click **✅ Apply to Inventory** to write the quantities

> **Cell size**: defaults to 64 px; adjust with the **＋/－** buttons to match your game resolution.  
> **Full scan**: if the game window has moved since the template was registered, enable **Full Scan** to search the entire screenshot.

---

## Build from Source 從原始碼建置

```bash
git clone https://github.com/EvansGoethe/Aurora-s-Bns-Material-Tracker.git
cd Aurora-s-Bns-Material-Tracker
dotnet publish -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o publish
```

需要 [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)。

如需重新打包安裝器，請安裝 [Inno Setup 6](https://jrsoftware.org/isdl.php) 後執行：

```bash
ISCC installer.iss
```

---

## Tech Stack

- C# / WPF (.NET 6.0)
- MVVM pattern
- System.Text.Json
- Windows.Media.Ocr (bag scanner OCR)
- [Inno Setup 6](https://jrsoftware.org/isdl.php) (installer)

---

*Made with ❤️ for the BnS TW community.*
