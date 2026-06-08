; *** Inno Setup version 6.0.0+ Chinese Traditional messages ***
; Traditional Chinese translation for Inno Setup

[LangOptions]
LanguageName=<4E2D><6587><FF08><7E41><9AD4><FF09>
LanguageID=$0404
LanguageCodePage=0

[Messages]
; *** Application titles
SetupAppTitle=安裝程式
SetupWindowTitle=%1 安裝程式
UninstallAppTitle=解除安裝
UninstallAppFullTitle=%1 解除安裝程式

; *** Misc. common
InformationTitle=資訊
ConfirmTitle=確認
ErrorTitle=錯誤

; *** SetupLdr messages
SetupLdrStartupMessage=本程式將安裝 %1。是否要繼續？
LdrCannotCreateTemp=無法建立暫存檔案。安裝中止。
LdrCannotExecTemp=無法執行暫存目錄中的檔案。安裝中止。
HelpTextNote=

; *** Startup error messages
LastErrorMessage=%1。%n%n錯誤 %2: %3
SetupFileMissing=安裝目錄中找不到 %1 檔案。請修正這個問題或取得新的程式副本。
SetupFileCorrupt=安裝檔案損毀。請取得新的程式副本。
SetupFileCorruptOrWrongVer=安裝檔案損毀，或與此版本的安裝程式不相容。請修正這個問題或取得新的程式副本。
InvalidParameter=命令列中傳遞了無效的參數：%n%n%1
SetupAlreadyRunning=安裝程式已在執行中。
WindowsVersionNotSupported=本程式不支援您電腦上執行的 Windows 版本。
WindowsServicePackRequired=本程式需要 %1 Service Pack %2 或更新版本。
NotOnThisPlatform=本程式無法在 %1 上執行。
OnlyOnThisPlatform=本程式只能在 %1 上執行。
OnlyOnTheseArchitectures=本程式只能安裝在以下處理器架構的 Windows 上：%n%n%1
WinVersionTooLowError=本程式需要 %1 %2 或更新版本。
WinVersionTooHighError=本程式無法在 %1 %2 或更新版本上安裝。
AdminPrivilegesRequired=安裝本程式時，您必須以系統管理員身分登入。
PowerUserPrivilegesRequired=安裝本程式時，您必須以系統管理員或 Power Users 群組成員的身分登入。
SetupAppRunningError=安裝程式偵測到 %1 目前正在執行中。%n%n請先關閉所有執行中的視窗，然後按確定繼續，或按取消結束安裝。
UninstallAppRunningError=解除安裝程式偵測到 %1 目前正在執行中。%n%n請先關閉所有執行中的視窗，然後按確定繼續，或按取消結束解除安裝。

; *** Startup questions
PrivilegesRequiredOverrideTitle=選擇安裝程式模式
PrivilegesRequiredOverrideInstruction=選擇安裝模式
PrivilegesRequiredOverrideText1=%1 可以為所有使用者安裝（需要系統管理員權限），或僅為您安裝。
PrivilegesRequiredOverrideText2=%1 可以僅為您安裝，或為所有使用者安裝（需要系統管理員權限）。
PrivilegesRequiredOverrideAllUsers=為所有使用者安裝(&A)
PrivilegesRequiredOverrideAllUsersRecommended=為所有使用者安裝(&A)（建議）
PrivilegesRequiredOverrideCurrentUser=僅為我安裝(&M)
PrivilegesRequiredOverrideCurrentUserRecommended=僅為我安裝(&M)（建議）

; *** Misc. errors
ErrorCreatingDir=安裝程式無法建立目錄「%1」。
ErrorTooManyFilesInDir=無法在目錄「%1」中建立檔案，因為目錄中包含太多檔案。

; *** Setup common messages
ExitSetupTitle=結束安裝程式
ExitSetupMessage=安裝尚未完成。如果您現在結束，程式將不會被安裝。%n%n您可以稍後再執行安裝程式。%n%n要結束安裝程式嗎？
AboutSetupMenuItem=關於安裝程式(&A)...
AboutSetupTitle=關於安裝程式
AboutSetupMessage=%1 版本 %2%n%3%n%n%1 首頁：%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< 上一步(&B)
ButtonNext=下一步(&N) >
ButtonInstall=安裝(&I)
ButtonOK=確定
ButtonCancel=取消
ButtonYes=是(&Y)
ButtonYesToAll=全部是(&A)
ButtonNo=否(&N)
ButtonNoToAll=全部否(&O)
ButtonFinish=完成(&F)
ButtonBrowse=瀏覽(&B)...
ButtonWizardBrowse=瀏覽(&R)...
ButtonNewFolder=建立新資料夾(&M)

; *** "Select Language" dialog messages
SelectLanguageTitle=選擇安裝程式語言
SelectLanguageLabel=請選擇安裝時使用的語言：

; *** Common wizard text
ClickNext=按 [下一步] 繼續，或按 [取消] 結束安裝程式。
BeveledLabel=
BrowseDialogTitle=瀏覽資料夾
BrowseDialogLabel=請在下面的清單中選取目標資料夾，然後按確定。
NewFolderName=新資料夾

; *** "Welcome" wizard page
WelcomeLabel1=歡迎使用 [name] 安裝精靈
WelcomeLabel2=本精靈將引導您完成 [name/ver] 的安裝。%n%n建議您在繼續之前先關閉其他所有應用程式。

; *** "Password" wizard page
WizardPassword=密碼
PasswordLabel1=本安裝程式有密碼保護。
PasswordLabel3=請輸入密碼，然後按 [下一步] 繼續。密碼區分大小寫。
PasswordEditLabel=密碼(&P)：
IncorrectPassword=您輸入的密碼不正確，請重試。

; *** "License Agreement" wizard page
WizardLicense=授權合約
LicenseLabel=繼續之前，請閱讀以下重要資訊。
LicenseLabel3=請閱讀下面的授權合約。您必須接受合約中的條款，才能繼續安裝。
LicenseAccepted=我接受合約(&A)
LicenseNotAccepted=我不接受合約(&D)

; *** "Information" wizard pages
WizardInfoBefore=資訊
InfoBeforeLabel=繼續之前，請閱讀以下重要資訊。
InfoBeforeClickLabel=當您準備好繼續安裝，請按 [下一步]。
WizardInfoAfter=資訊
InfoAfterLabel=繼續之前，請閱讀以下重要資訊。
InfoAfterClickLabel=當您準備好繼續安裝，請按 [下一步]。

; *** "User Information" wizard page
WizardUserInfo=使用者資訊
UserInfoDesc=請輸入您的資訊。
UserInfoName=使用者名稱(&U)：
UserInfoOrg=組織(&O)：
UserInfoSerial=序號(&S)：
UserInfoNameRequired=您必須輸入使用者名稱。

; *** "Select Destination Location" wizard page
WizardSelectDir=選擇安裝位置
SelectDirDesc=要把 [name] 安裝在哪裡？
SelectDirLabel3=安裝程式將把 [name] 安裝在以下資料夾中。
SelectDirBrowseLabel=要繼續，請按 [下一步]。如果您想選擇其他資料夾，請按 [瀏覽]。
DiskSpaceGBLabel=安裝程式至少需要 [gb] GB 的可用磁碟空間。
DiskSpaceMBLabel=安裝程式至少需要 [mb] MB 的可用磁碟空間。
CannotInstallToNetworkDrive=安裝程式無法安裝至網路磁碟機。
CannotInstallToUNCPath=安裝程式無法安裝至 UNC 路徑。
InvalidPath=您必須輸入附有磁碟機代號的完整路徑，例如：%n%nC:\APP%n%n或 UNC 路徑：%n%n\\server\share
InvalidDrive=您選取的磁碟機或 UNC 共用不存在或無法存取，請選擇其他位置。
DiskSpaceWarningTitle=磁碟空間不足
DiskSpaceWarning=安裝程式至少需要 %1 KB 的可用磁碟空間，但選取的磁碟機只有 %2 KB 可用。%n%n是否仍要繼續？
DirNameTooLong=資料夾名稱或路徑太長。
InvalidDirName=資料夾名稱無效。
BadDirName32=資料夾名稱不能包含下列字元：%n%n%1
DirExistsTitle=資料夾已存在
DirExists=資料夾：%n%n%1%n%n已存在。是否仍要安裝到這個資料夾？
DirDoesntExistTitle=資料夾不存在
DirDoesntExist=資料夾：%n%n%1%n%n不存在。是否要建立這個資料夾？

; *** "Select Components" wizard page
WizardSelectComponents=選擇元件
SelectComponentsDesc=要安裝哪些程式元件？
SelectComponentsLabel2=選取您想安裝的元件，清除您不想安裝的元件。完成後按 [下一步] 繼續。
FullInstallation=完整安裝
CompactInstallation=精簡安裝
CustomInstallation=自訂安裝
NoUninstallWarningTitle=元件已存在
NoUninstallWarning=安裝程式偵測到以下元件已安裝在您的電腦上：%n%n%1%n%n取消選取這些元件將不會將其解除安裝。%n%n是否仍要繼續？
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceGBLabel=目前選取的元件至少需要 [gb] GB 的磁碟空間。
ComponentsDiskSpaceMBLabel=目前選取的元件至少需要 [mb] MB 的磁碟空間。

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=選擇附加工作
SelectTasksDesc=要執行哪些附加工作？
SelectTasksLabel2=選擇安裝 [name] 時要執行的附加工作，然後按 [下一步] 繼續。

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=選擇開始功能表資料夾
SelectStartMenuFolderDesc=安裝程式要在哪裡建立程式的捷徑？
SelectStartMenuFolderLabel3=安裝程式將在以下開始功能表資料夾中建立程式的捷徑。
SelectStartMenuFolderBrowseLabel=要繼續，請按 [下一步]。如果您想選擇其他資料夾，請按 [瀏覽]。
MustEnterGroupName=您必須輸入資料夾名稱。
GroupNameTooLong=資料夾名稱或路徑太長。
InvalidGroupName=資料夾名稱無效。
BadGroupName=資料夾名稱不能包含下列字元：%n%n%1
NoProgramGroupCheck2=不要在開始功能表中建立資料夾(&D)

; *** "Ready to Install" wizard page
WizardReady=準備安裝
ReadyLabel1=安裝程式已準備好在您的電腦上安裝 [name]。
ReadyLabel2a=按 [安裝] 繼續，或按 [上一步] 檢視或變更設定。
ReadyLabel2b=按 [安裝] 繼續安裝。
ReadyMemoUserInfo=使用者資訊：
ReadyMemoDir=目標位置：
ReadyMemoType=安裝類型：
ReadyMemoComponents=已選取的元件：
ReadyMemoGroup=開始功能表資料夾：
ReadyMemoTasks=附加工作：

; *** TDownloadWizardPage
DownloadingLabel=正在下載其他檔案...
ButtonStopDownload=停止下載(&S)
StopDownload=確定要停止下載嗎？
ErrorDownloadAborted=下載已中止
ErrorDownloadFailed=下載失敗：%1 %2
ErrorDownloadSizeFailed=取得大小失敗：%1 %2
ErrorFileHash1=檔案雜湊驗證失敗：%1
ErrorFileHash2=無效的檔案雜湊：預期 %1，實際 %2
ErrorProgress=無效的進度：%1 / %2
ErrorFileSize=無效的檔案大小：預期 %1，實際 %2

; *** "Preparing to Install" wizard page
WizardPreparing=準備安裝
PreparingDesc=安裝程式正在準備安裝 [name] 到您的電腦上。
PreviousInstallNotCompleted=前一個程式的安裝/移除未完成。請重新啟動電腦以完成該安裝，然後再次執行本安裝程式。
CannotContinue=安裝程式無法繼續。請按 [取消] 結束安裝。
ApplicationsFound=以下應用程式正在使用需要更新的檔案。建議您允許安裝程式自動關閉這些應用程式。
ApplicationsFound2=以下應用程式正在使用需要更新的檔案。建議您允許安裝程式自動關閉這些應用程式。安裝完成後，安裝程式將嘗試重新啟動這些應用程式。
CloseApplications=自動關閉應用程式(&A)
DontCloseApplications=不要關閉應用程式(&D)
ErrorCloseApplications=安裝程式無法自動關閉所有應用程式。請手動關閉後再繼續。
PrepareToInstallNeedsRestart=安裝程式必須重新啟動您的電腦。重新啟動後，請再次執行安裝程式以完成安裝。%n%n現在要重新啟動嗎？

; *** "Installing" wizard page
WizardInstalling=正在安裝
InstallingLabel=安裝程式正在安裝 [name] 到您的電腦上，請稍等。

; *** "Setup Completed" wizard page
FinishedHeadingLabel=完成 [name] 安裝精靈
FinishedLabelNoIcons=已完成 [name] 的安裝。
FinishedLabel=已完成 [name] 的安裝。選取下面的捷徑圖示以啟動程式。
ClickFinish=按 [完成] 結束安裝程式。
FinishedRestartLabel=要完成 [name] 的安裝，需要重新啟動您的電腦。要現在重新啟動嗎？
FinishedRestartMessage=要完成 [name] 的安裝，需要重新啟動您的電腦。%n%n要現在重新啟動嗎？
ShowReadmeCheck=是的，我想查看 README 檔案
YesRadio=是的，立即重新啟動電腦(&Y)
NoRadio=否，稍後再重新啟動電腦(&N)
RunEntryExec=執行 %1
RunEntryShellExec=查看 %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=安裝程式需要下一張磁片
SelectDiskLabel2=請插入磁片 %1 並按確定。%n%n如果檔案不在以下資料夾中，請輸入正確的路徑或按 [瀏覽]。
PathLabel=路徑(&P)：
FileNotInDir2=在「%2」中找不到「%1」檔案。請插入正確的磁片或選擇其他資料夾。
SelectDirectoryLabel=請指定下一張磁片的位置。

; *** Installation phase messages
SetupAborted=安裝未完成。%n%n請修正這個問題並重新執行安裝程式。
AbortRetryIgnoreSelectAction=選擇動作
AbortRetryIgnoreRetry=再試一次(&T)
AbortRetryIgnoreIgnore=略過錯誤繼續(&I)
AbortRetryIgnoreCancel=取消安裝

; *** Installation status messages
StatusClosingApplications=正在關閉應用程式...
StatusCreateDirs=正在建立目錄...
StatusExtractFiles=正在解壓縮檔案...
StatusCreateIcons=正在建立捷徑...
StatusCreateIniEntries=正在建立 INI 項目...
StatusCreateRegistryEntries=正在建立登錄項目...
StatusRegisterFiles=正在登錄檔案...
StatusSavingUninstall=正在儲存解除安裝資訊...
StatusRunProgram=正在完成安裝...
StatusRestartingApplications=正在重新啟動應用程式...
StatusRollback=正在回復變更...

; *** Misc. errors
ErrorInternal2=內部錯誤：%1
ErrorFunctionFailedNoCode=%1 失敗
ErrorFunctionFailed=%1 失敗；代碼 %2
ErrorFunctionFailedWithMessage=%1 失敗；代碼 %2。%n%3
ErrorExecutingProgram=無法執行檔案：%n%1

; *** Registry errors
ErrorRegOpenKey=開啟登錄機碼時發生錯誤：%n%1\%2
ErrorRegCreateKey=建立登錄機碼時發生錯誤：%n%1\%2
ErrorRegWriteKey=寫入登錄機碼時發生錯誤：%n%1\%2

; *** INI errors
ErrorIniEntry=在「%1」檔案中建立 INI 項目時發生錯誤。

; *** File copying errors
FileAbortRetryIgnoreSkipNotRecommended=略過這個檔案（不建議）(&S)
FileAbortRetryIgnoreIgnoreNotRecommended=略過錯誤繼續（不建議）(&I)
SourceIsCorrupted=來源檔案損毀
SourceDoesntExist=來源檔案「%1」不存在
ExistingFileReadOnly2=無法取代現有的檔案，因為它是唯讀的。
ExistingFileReadOnlyRetry=移除唯讀屬性並再試一次(&R)
ExistingFileReadOnlyKeepExisting=保留現有的檔案(&K)
ErrorReadingExistingDest=嘗試讀取現有的檔案時發生錯誤：
FileExists=檔案已存在。%n%n安裝程式要覆蓋這個檔案嗎？
ExistingFileOlder=現有的檔案比要安裝的檔案舊。建議保留現有的檔案。%n%n要保留現有的檔案嗎？
ErrorChangingAttr=嘗試變更現有的檔案屬性時發生錯誤：
ErrorCreatingTemp=嘗試在目標目錄中建立檔案時發生錯誤：
ErrorReadingSource=嘗試讀取來源檔案時發生錯誤：
ErrorCopying=嘗試複製檔案時發生錯誤：
ErrorReplacingExistingFile=嘗試取代現有的檔案時發生錯誤：
ErrorRestartReplace=重新啟動後取代失敗：
ErrorRenamingTemp=嘗試重新命名目標目錄中的檔案時發生錯誤：
ErrorRegisterServer=無法登錄 DLL/OCX：%1
ErrorRegSvr32Failed=RegSvr32 失敗，結束代碼 %1
ErrorRegisterTypeLib=無法登錄型別程式庫：%1

; *** Uninstall display name markings
UninstallDisplayNameMark=%1 (%2)
UninstallDisplayNameMarks=%1 (%2, %3)
UninstallDisplayNameMark32Bit=32 位元
UninstallDisplayNameMark64Bit=64 位元
UninstallDisplayNameMarkAllUsers=所有使用者
UninstallDisplayNameMarkCurrentUser=目前使用者

; *** Post-installation errors
ErrorOpeningReadme=開啟 README 檔案時發生錯誤。
ErrorRestartingComputer=安裝程式無法重新啟動電腦，請手動重新啟動。

; *** Uninstaller messages
UninstallNotFound=「%1」檔案不存在，無法解除安裝。
UninstallOpenError=「%1」檔案無法開啟，無法解除安裝。
UninstallUnsupportedVer=此版本的解除安裝程式無法辨識「%1」解除安裝記錄檔的格式。
UninstallUnknownEntry=解除安裝記錄中有不明的項目 (%1)
ConfirmUninstall=您確定要完全移除 %1 及其所有元件？
UninstallOnlyOnWin64=只能在 64 位元的 Windows 上解除安裝這個程式。
OnlyAdminCanUninstall=只有具備系統管理員權限的使用者才能解除安裝這個程式。
UninstallStatusLabel=正在從您的電腦移除 %1，請稍等。
UninstalledAll=%1 已成功從您的電腦移除。
UninstalledMost=%1 解除安裝完成。%n%n有些項目無法被移除，您可以手動刪除它們。
UninstalledAndNeedsRestart=要完成解除安裝 %1，您需要重新啟動電腦。%n%n要現在重新啟動嗎？
UninstallDataCorrupted=「%1」檔案損毀，無法解除安裝。

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=要移除共用檔案嗎？
ConfirmDeleteSharedFile2=系統指出以下共用檔案已不再被任何程式使用。是否要移除這些共用檔案？%n%n如果仍有程式在使用這些檔案，移除後那些程式可能無法正常執行。如果不確定，請選擇 [否]。
SharedFileNameLabel=檔案名稱：
SharedFileLocationLabel=位置：
WizardUninstalling=解除安裝狀態
StatusUninstalling=正在解除安裝 %1...

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=正在安裝 %1。
ShutdownBlockReasonUninstallingApp=正在解除安裝 %1。

[CustomMessages]
NameAndVersion=%1 版本 %2
AdditionalIcons=附加捷徑：
CreateDesktopIcon=建立桌面捷徑(&D)
CreateQuickLaunchIcon=建立快速啟動捷徑(&Q)
ProgramOnTheWeb=%1 網站
UninstallProgram=解除安裝 %1
LaunchProgram=啟動 %1
AssocFileExtension=將 %1 與 %2 副檔名建立關聯(&A)
AssocingFileExtension=正在將 %1 與 %2 副檔名建立關聯...
AutoStartProgramGroupDescription=啟動：
AutoStartProgram=自動啟動 %1
AddonHostProgramNotFound=找不到 %1。%n%n是否仍要繼續？
