; *** Inno Setup version 6.0.0+ Chinese Simplified messages ***

[LangOptions]
LanguageName=<4E2D><6587><FF08><7B80><4F53><FF09>
LanguageID=$0804
LanguageCodePage=0

[Messages]
; *** Application titles
SetupAppTitle=安装程序
SetupWindowTitle=%1 安装程序
UninstallAppTitle=卸载
UninstallAppFullTitle=%1 卸载程序

; *** Misc. common
InformationTitle=信息
ConfirmTitle=确认
ErrorTitle=错误

; *** SetupLdr messages
SetupLdrStartupMessage=本程序将安装 %1。是否继续？
LdrCannotCreateTemp=无法创建临时文件。安装中止。
LdrCannotExecTemp=无法执行临时目录中的文件。安装中止。
HelpTextNote=

; *** Startup error messages
LastErrorMessage=%1。%n%n错误 %2: %3
SetupFileMissing=安装目录中找不到 %1 文件。请修正此问题或获取新的程序副本。
SetupFileCorrupt=安装文件已损坏。请获取新的程序副本。
SetupFileCorruptOrWrongVer=安装文件已损坏，或与此版本的安装程序不兼容。请修正此问题或获取新的程序副本。
InvalidParameter=命令行中传递了无效的参数：%n%n%1
SetupAlreadyRunning=安装程序已在运行。
WindowsVersionNotSupported=本程序不支持您计算机上运行的 Windows 版本。
WindowsServicePackRequired=本程序需要 %1 Service Pack %2 或更高版本。
NotOnThisPlatform=本程序无法在 %1 上运行。
OnlyOnThisPlatform=本程序只能在 %1 上运行。
OnlyOnTheseArchitectures=本程序只能安装在以下处理器架构的 Windows 上：%n%n%1
WinVersionTooLowError=本程序需要 %1 %2 或更高版本。
WinVersionTooHighError=本程序无法在 %1 %2 或更高版本上安装。
AdminPrivilegesRequired=安装本程序时，您必须以管理员身份登录。
PowerUserPrivilegesRequired=安装本程序时，您必须以管理员或 Power Users 组成员的身份登录。
SetupAppRunningError=安装程序检测到 %1 当前正在运行。%n%n请先关闭所有运行中的窗口，然后单击"确定"继续，或单击"取消"退出安装。
UninstallAppRunningError=卸载程序检测到 %1 当前正在运行。%n%n请先关闭所有运行中的窗口，然后单击"确定"继续，或单击"取消"退出卸载。

; *** Startup questions
PrivilegesRequiredOverrideTitle=选择安装程序模式
PrivilegesRequiredOverrideInstruction=选择安装模式
PrivilegesRequiredOverrideText1=%1 可以为所有用户安装（需要管理员权限），或仅为您安装。
PrivilegesRequiredOverrideText2=%1 可以仅为您安装，或为所有用户安装（需要管理员权限）。
PrivilegesRequiredOverrideAllUsers=为所有用户安装(&A)
PrivilegesRequiredOverrideAllUsersRecommended=为所有用户安装(&A)（推荐）
PrivilegesRequiredOverrideCurrentUser=仅为我安装(&M)
PrivilegesRequiredOverrideCurrentUserRecommended=仅为我安装(&M)（推荐）

; *** Misc. errors
ErrorCreatingDir=安装程序无法创建目录"%1"。
ErrorTooManyFilesInDir=无法在目录"%1"中创建文件，因为目录中包含太多文件。

; *** Setup common messages
ExitSetupTitle=退出安装程序
ExitSetupMessage=安装尚未完成。如果您现在退出，程序将不会被安装。%n%n您可以稍后再运行安装程序。%n%n要退出安装程序吗？
AboutSetupMenuItem=关于安装程序(&A)...
AboutSetupTitle=关于安装程序
AboutSetupMessage=%1 版本 %2%n%3%n%n%1 主页：%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< 上一步(&B)
ButtonNext=下一步(&N) >
ButtonInstall=安装(&I)
ButtonOK=确定
ButtonCancel=取消
ButtonYes=是(&Y)
ButtonYesToAll=全部是(&A)
ButtonNo=否(&N)
ButtonNoToAll=全部否(&O)
ButtonFinish=完成(&F)
ButtonBrowse=浏览(&B)...
ButtonWizardBrowse=浏览(&R)...
ButtonNewFolder=新建文件夹(&M)

; *** "Select Language" dialog messages
SelectLanguageTitle=选择安装程序语言
SelectLanguageLabel=请选择安装时使用的语言：

; *** Common wizard text
ClickNext=单击"下一步"继续，或单击"取消"退出安装程序。
BeveledLabel=
BrowseDialogTitle=浏览文件夹
BrowseDialogLabel=请在下面的列表中选择目标文件夹，然后单击"确定"。
NewFolderName=新建文件夹

; *** "Welcome" wizard page
WelcomeLabel1=欢迎使用 [name] 安装向导
WelcomeLabel2=本向导将引导您完成 [name/ver] 的安装。%n%n建议您在继续之前先关闭其他所有应用程序。

; *** "Password" wizard page
WizardPassword=密码
PasswordLabel1=本安装程序受密码保护。
PasswordLabel3=请输入密码，然后单击"下一步"继续。密码区分大小写。
PasswordEditLabel=密码(&P)：
IncorrectPassword=您输入的密码不正确，请重试。

; *** "License Agreement" wizard page
WizardLicense=许可协议
LicenseLabel=继续之前，请阅读以下重要信息。
LicenseLabel3=请阅读以下许可协议。您必须接受协议中的条款，才能继续安装。
LicenseAccepted=我接受协议(&A)
LicenseNotAccepted=我不接受协议(&D)

; *** "Information" wizard pages
WizardInfoBefore=信息
InfoBeforeLabel=继续之前，请阅读以下重要信息。
InfoBeforeClickLabel=准备好继续安装后，请单击"下一步"。
WizardInfoAfter=信息
InfoAfterLabel=继续之前，请阅读以下重要信息。
InfoAfterClickLabel=准备好继续安装后，请单击"下一步"。

; *** "User Information" wizard page
WizardUserInfo=用户信息
UserInfoDesc=请输入您的信息。
UserInfoName=用户名(&U)：
UserInfoOrg=组织(&O)：
UserInfoSerial=序列号(&S)：
UserInfoNameRequired=您必须输入用户名。

; *** "Select Destination Location" wizard page
WizardSelectDir=选择安装位置
SelectDirDesc=要将 [name] 安装在哪里？
SelectDirLabel3=安装程序将把 [name] 安装在以下文件夹中。
SelectDirBrowseLabel=要继续，请单击"下一步"。如果您想选择其他文件夹，请单击"浏览"。
DiskSpaceGBLabel=安装程序至少需要 [gb] GB 的可用磁盘空间。
DiskSpaceMBLabel=安装程序至少需要 [mb] MB 的可用磁盘空间。
CannotInstallToNetworkDrive=安装程序无法安装至网络驱动器。
CannotInstallToUNCPath=安装程序无法安装至 UNC 路径。
InvalidPath=您必须输入带有驱动器号的完整路径，例如：%n%nC:\APP%n%n或以下格式的 UNC 路径：%n%n\\server\share
InvalidDrive=您选择的驱动器或 UNC 共享不存在或无法访问，请选择其他位置。
DiskSpaceWarningTitle=磁盘空间不足
DiskSpaceWarning=安装程序至少需要 %1 KB 的可用磁盘空间，但选择的驱动器只有 %2 KB 可用。%n%n是否仍要继续？
DirNameTooLong=文件夹名称或路径太长。
InvalidDirName=文件夹名称无效。
BadDirName32=文件夹名称不能包含以下字符：%n%n%1
DirExistsTitle=文件夹已存在
DirExists=文件夹：%n%n%1%n%n已存在。是否仍要安装到此文件夹？
DirDoesntExistTitle=文件夹不存在
DirDoesntExist=文件夹：%n%n%1%n%n不存在。是否要创建此文件夹？

; *** "Select Components" wizard page
WizardSelectComponents=选择组件
SelectComponentsDesc=要安装哪些程序组件？
SelectComponentsLabel2=选择您想安装的组件，清除您不想安装的组件。完成后单击"下一步"继续。
FullInstallation=完整安装
CompactInstallation=精简安装
CustomInstallation=自定义安装
NoUninstallWarningTitle=组件已存在
NoUninstallWarning=安装程序检测到以下组件已安装在您的计算机上：%n%n%1%n%n取消选择这些组件不会卸载它们。%n%n是否仍要继续？
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceGBLabel=当前选择的组件至少需要 [gb] GB 的磁盘空间。
ComponentsDiskSpaceMBLabel=当前选择的组件至少需要 [mb] MB 的磁盘空间。

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=选择附加任务
SelectTasksDesc=要执行哪些附加任务？
SelectTasksLabel2=选择安装 [name] 时要执行的附加任务，然后单击"下一步"继续。

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=选择开始菜单文件夹
SelectStartMenuFolderDesc=安装程序要在哪里创建程序的快捷方式？
SelectStartMenuFolderLabel3=安装程序将在以下开始菜单文件夹中创建程序的快捷方式。
SelectStartMenuFolderBrowseLabel=要继续，请单击"下一步"。如果您想选择其他文件夹，请单击"浏览"。
MustEnterGroupName=您必须输入文件夹名称。
GroupNameTooLong=文件夹名称或路径太长。
InvalidGroupName=文件夹名称无效。
BadGroupName=文件夹名称不能包含以下字符：%n%n%1
NoProgramGroupCheck2=不在开始菜单中创建文件夹(&D)

; *** "Ready to Install" wizard page
WizardReady=准备安装
ReadyLabel1=安装程序已准备好在您的计算机上安装 [name]。
ReadyLabel2a=单击"安装"继续，或单击"上一步"查看或更改设置。
ReadyLabel2b=单击"安装"继续安装。
ReadyMemoUserInfo=用户信息：
ReadyMemoDir=目标位置：
ReadyMemoType=安装类型：
ReadyMemoComponents=已选择的组件：
ReadyMemoGroup=开始菜单文件夹：
ReadyMemoTasks=附加任务：

; *** TDownloadWizardPage
DownloadingLabel=正在下载附加文件...
ButtonStopDownload=停止下载(&S)
StopDownload=确定要停止下载吗？
ErrorDownloadAborted=下载已中止
ErrorDownloadFailed=下载失败：%1 %2
ErrorDownloadSizeFailed=获取大小失败：%1 %2
ErrorFileHash1=文件哈希验证失败：%1
ErrorFileHash2=无效的文件哈希：预期 %1，实际 %2
ErrorProgress=无效的进度：%1 / %2
ErrorFileSize=无效的文件大小：预期 %1，实际 %2

; *** "Preparing to Install" wizard page
WizardPreparing=准备安装
PreparingDesc=安装程序正在准备将 [name] 安装到您的计算机上。
PreviousInstallNotCompleted=上一个程序的安装/删除未完成。请重新启动计算机以完成该安装，然后再次运行本安装程序。
CannotContinue=安装程序无法继续。请单击"取消"退出安装。
ApplicationsFound=以下应用程序正在使用需要更新的文件。建议您允许安装程序自动关闭这些应用程序。
ApplicationsFound2=以下应用程序正在使用需要更新的文件。建议您允许安装程序自动关闭这些应用程序。安装完成后，安装程序将尝试重新启动这些应用程序。
CloseApplications=自动关闭应用程序(&A)
DontCloseApplications=不关闭应用程序(&D)
ErrorCloseApplications=安装程序无法自动关闭所有应用程序。请手动关闭后再继续。
PrepareToInstallNeedsRestart=安装程序必须重新启动您的计算机。重新启动后，请再次运行安装程序以完成安装。%n%n现在要重新启动吗？

; *** "Installing" wizard page
WizardInstalling=正在安装
InstallingLabel=安装程序正在将 [name] 安装到您的计算机上，请稍候。

; *** "Setup Completed" wizard page
FinishedHeadingLabel=完成 [name] 安装向导
FinishedLabelNoIcons=已完成 [name] 的安装。
FinishedLabel=已完成 [name] 的安装。选择下面的快捷方式图标以启动程序。
ClickFinish=单击"完成"退出安装程序。
FinishedRestartLabel=要完成 [name] 的安装，需要重新启动您的计算机。要现在重新启动吗？
FinishedRestartMessage=要完成 [name] 的安装，需要重新启动您的计算机。%n%n要现在重新启动吗？
ShowReadmeCheck=是的，我想查看 README 文件
YesRadio=是，立即重新启动计算机(&Y)
NoRadio=否，稍后再重新启动计算机(&N)
RunEntryExec=运行 %1
RunEntryShellExec=查看 %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=安装程序需要下一张磁盘
SelectDiskLabel2=请插入磁盘 %1 并单击"确定"。%n%n如果文件不在以下文件夹中，请输入正确路径或单击"浏览"。
PathLabel=路径(&P)：
FileNotInDir2=在"%2"中找不到"%1"文件。请插入正确的磁盘或选择其他文件夹。
SelectDirectoryLabel=请指定下一张磁盘的位置。

; *** Installation phase messages
SetupAborted=安装未完成。%n%n请修正此问题并重新运行安装程序。
AbortRetryIgnoreSelectAction=选择操作
AbortRetryIgnoreRetry=重试(&T)
AbortRetryIgnoreIgnore=忽略错误继续(&I)
AbortRetryIgnoreCancel=取消安装

; *** Installation status messages
StatusClosingApplications=正在关闭应用程序...
StatusCreateDirs=正在创建目录...
StatusExtractFiles=正在解压缩文件...
StatusCreateIcons=正在创建快捷方式...
StatusCreateIniEntries=正在创建 INI 条目...
StatusCreateRegistryEntries=正在创建注册表条目...
StatusRegisterFiles=正在注册文件...
StatusSavingUninstall=正在保存卸载信息...
StatusRunProgram=正在完成安装...
StatusRestartingApplications=正在重新启动应用程序...
StatusRollback=正在回滚更改...

; *** Misc. errors
ErrorInternal2=内部错误：%1
ErrorFunctionFailedNoCode=%1 失败
ErrorFunctionFailed=%1 失败；代码 %2
ErrorFunctionFailedWithMessage=%1 失败；代码 %2。%n%3
ErrorExecutingProgram=无法执行文件：%n%1

; *** Registry errors
ErrorRegOpenKey=打开注册表键时出错：%n%1\%2
ErrorRegCreateKey=创建注册表键时出错：%n%1\%2
ErrorRegWriteKey=写入注册表键时出错：%n%1\%2

; *** INI errors
ErrorIniEntry=在"%1"文件中创建 INI 条目时出错。

; *** File copying errors
FileAbortRetryIgnoreSkipNotRecommended=跳过此文件（不推荐）(&S)
FileAbortRetryIgnoreIgnoreNotRecommended=忽略错误继续（不推荐）(&I)
SourceIsCorrupted=源文件已损坏
SourceDoesntExist=源文件"%1"不存在
ExistingFileReadOnly2=无法替换现有文件，因为它是只读的。
ExistingFileReadOnlyRetry=删除只读属性并重试(&R)
ExistingFileReadOnlyKeepExisting=保留现有文件(&K)
ErrorReadingExistingDest=尝试读取现有文件时出错：
FileExists=文件已存在。%n%n安装程序要覆盖此文件吗？
ExistingFileOlder=现有文件比要安装的文件旧。建议保留现有文件。%n%n要保留现有文件吗？
ErrorChangingAttr=尝试更改现有文件属性时出错：
ErrorCreatingTemp=尝试在目标目录中创建文件时出错：
ErrorReadingSource=尝试读取源文件时出错：
ErrorCopying=尝试复制文件时出错：
ErrorReplacingExistingFile=尝试替换现有文件时出错：
ErrorRestartReplace=重新启动后替换失败：
ErrorRenamingTemp=尝试重命名目标目录中的文件时出错：
ErrorRegisterServer=无法注册 DLL/OCX：%1
ErrorRegSvr32Failed=RegSvr32 失败，退出代码 %1
ErrorRegisterTypeLib=无法注册类型库：%1

; *** Uninstall display name markings
UninstallDisplayNameMark=%1 (%2)
UninstallDisplayNameMarks=%1 (%2, %3)
UninstallDisplayNameMark32Bit=32 位
UninstallDisplayNameMark64Bit=64 位
UninstallDisplayNameMarkAllUsers=所有用户
UninstallDisplayNameMarkCurrentUser=当前用户

; *** Post-installation errors
ErrorOpeningReadme=打开 README 文件时出错。
ErrorRestartingComputer=安装程序无法重新启动计算机，请手动重新启动。

; *** Uninstaller messages
UninstallNotFound="%1"文件不存在，无法卸载。
UninstallOpenError="%1"文件无法打开，无法卸载。
UninstallUnsupportedVer=此版本的卸载程序无法识别"%1"卸载日志文件的格式。
UninstallUnknownEntry=卸载日志中有未知条目 (%1)
ConfirmUninstall=您确定要完全删除 %1 及其所有组件吗？
UninstallOnlyOnWin64=只能在 64 位 Windows 上卸载此程序。
OnlyAdminCanUninstall=只有具备管理员权限的用户才能卸载此程序。
UninstallStatusLabel=正在从您的计算机中删除 %1，请稍候。
UninstalledAll=%1 已成功从您的计算机中删除。
UninstalledMost=%1 卸载完成。%n%n有些项目无法被删除，您可以手动删除它们。
UninstalledAndNeedsRestart=要完成卸载 %1，您需要重新启动计算机。%n%n要现在重新启动吗？
UninstallDataCorrupted="%1"文件已损坏，无法卸载。

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=删除共享文件？
ConfirmDeleteSharedFile2=系统指出以下共享文件已不再被任何程序使用。是否要删除这些共享文件？%n%n如果仍有程序使用这些文件，删除后那些程序可能无法正常运行。如果不确定，请选择"否"。
SharedFileNameLabel=文件名：
SharedFileLocationLabel=位置：
WizardUninstalling=卸载状态
StatusUninstalling=正在卸载 %1...

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=正在安装 %1。
ShutdownBlockReasonUninstallingApp=正在卸载 %1。

[CustomMessages]
NameAndVersion=%1 版本 %2
AdditionalIcons=附加快捷方式：
CreateDesktopIcon=创建桌面快捷方式(&D)
CreateQuickLaunchIcon=创建快速启动快捷方式(&Q)
ProgramOnTheWeb=%1 网站
UninstallProgram=卸载 %1
LaunchProgram=启动 %1
AssocFileExtension=将 %1 与 %2 扩展名关联(&A)
AssocingFileExtension=正在将 %1 与 %2 扩展名关联...
AutoStartProgramGroupDescription=启动：
AutoStartProgram=自动启动 %1
AddonHostProgramNotFound=找不到 %1。%n%n是否仍要继续？
