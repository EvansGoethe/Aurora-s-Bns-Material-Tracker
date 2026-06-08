# Aurora's BnS Material Tracker — 洛洛劍靈材料追蹤器

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
| **多語言** | 繁體中文 / 簡體中文 / English 即時切換 |
| **i18n** | Traditional Chinese / Simplified Chinese / English |

---

## Requirements 執行需求

- Windows 10 / 11 (x64)
- [.NET 6.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

---

## Installation 安裝方式

1. 至 [Releases](../../releases) 下載最新版本的 `洛洛劍靈材料追蹤器.exe`
2. 將 exe 與 `Data/` 資料夾放在同一目錄下
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

---

## Build from Source 從原始碼建置

```bash
git clone https://github.com/EvansGoethe/Aurora-s-Bns-Material-Tracker.git
cd Aurora-s-Bns-Material-Tracker
dotnet publish -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o publish
```

需要 [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)。

---

## Tech Stack

- C# / WPF (.NET 6.0)
- MVVM pattern
- System.Text.Json

---

*Made with ❤️ for the BnS TW community.*
