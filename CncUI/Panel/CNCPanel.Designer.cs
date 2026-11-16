namespace CncUI
{
    partial class CNCPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.m_pnlCommands = new System.Windows.Forms.Panel();
            this.m_grpAuxiliary = new CommonUI.Controls.SectionPanel();
            this.m_lblAuxiliaryTitle = new CommonUI.Controls.ThemedLabel();
            this.m_btnOuterEquipParameters = new CommonUI.Controls.ThemedButton();
            this.m_btnShutter = new CommonUI.Controls.ThemedButton();
            this.m_btnAir = new CommonUI.Controls.ThemedButton();
            this.m_btnFollow = new CommonUI.Controls.ThemedButton();
            this.m_btnLaser = new CommonUI.Controls.ThemedButton();
            this.m_btnFocus = new CommonUI.Controls.ThemedButton();
            this.m_btnCalibration = new CommonUI.Controls.ThemedButton();
            this.m_grpJog = new CommonUI.Controls.SectionPanel();
            this.m_lblJogTitle = new CommonUI.Controls.ThemedLabel();
            this.m_btnJogStep5 = new CommonUI.Controls.ThemedButton();
            this.m_btnJogStep10 = new CommonUI.Controls.ThemedButton();
            this.m_btnJogStep50 = new CommonUI.Controls.ThemedButton();
            this.m_btnJogStep100 = new CommonUI.Controls.ThemedButton();
            this.m_btnJogStep200 = new CommonUI.Controls.ThemedButton();
            this.m_btnZPlus = new CommonUI.Controls.ThemedButton();
            this.m_btnYPlus = new CommonUI.Controls.ThemedButton();
            this.m_btnBPlus = new CommonUI.Controls.ThemedButton();
            this.m_btnXMinus = new CommonUI.Controls.ThemedButton();
            this.m_btnJog = new CommonUI.Controls.ThemedButton();
            this.m_btnXPlus = new CommonUI.Controls.ThemedButton();
            this.m_btnZMinus = new CommonUI.Controls.ThemedButton();
            this.m_btnYMinus = new CommonUI.Controls.ThemedButton();
            this.m_btnBMinus = new CommonUI.Controls.ThemedButton();
            this.m_btnCMinus = new CommonUI.Controls.ThemedButton();
            this.m_btnEndpoint = new CommonUI.Controls.ThemedButton();
            this.m_btnCPlus = new CommonUI.Controls.ThemedButton();
            this.m_grpHome = new CommonUI.Controls.SectionPanel();
            this.m_lblHomeTitle = new CommonUI.Controls.ThemedLabel();
            this.m_btnHomeProgram = new CommonUI.Controls.ThemedButton();
            this.m_btnHomeMachine = new CommonUI.Controls.ThemedButton();
            this.m_btnSafePosition = new CommonUI.Controls.ThemedButton();
            this.m_grpStrategy = new CommonUI.Controls.SectionPanel();
            this.m_lblStrategyTitle = new CommonUI.Controls.ThemedLabel();
            this.m_btnResumeCutParameters = new CommonUI.Controls.ThemedButton();
            this.m_btnDryRun = new CommonUI.Controls.ThemedButton();
            this.m_btnResumeCutting = new CommonUI.Controls.ThemedButton();
            this.m_btnHandwheelSimulation = new CommonUI.Controls.ThemedButton();
            this.m_grpMachining = new CommonUI.Controls.SectionPanel();
            this.m_lblMachiningTitle = new CommonUI.Controls.ThemedLabel();
            this.m_chbThreePointCalibration = new System.Windows.Forms.CheckBox();
            this.m_lblFeedOverrideTitle = new CommonUI.Controls.ThemedLabel();
            this.m_lblFeedOverrideValue = new CommonUI.Controls.ThemedLabel();
            this.m_trbFeedOverride = new System.Windows.Forms.TrackBar();
            this.m_btnStart = new CommonUI.Controls.ThemedButton();
            this.m_btnPause = new CommonUI.Controls.ThemedButton();
            this.m_btnContinue = new CommonUI.Controls.ThemedButton();
            this.m_btnStop = new CommonUI.Controls.ThemedButton();
            this.m_pnlCommands.SuspendLayout();
            this.m_grpAuxiliary.SuspendLayout();
            this.m_grpJog.SuspendLayout();
            this.m_grpHome.SuspendLayout();
            this.m_grpStrategy.SuspendLayout();
            this.m_grpMachining.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.m_trbFeedOverride ) ).BeginInit();
            this.SuspendLayout();
            //
            // m_pnlCommands
            //
            this.m_pnlCommands.Controls.Add( this.m_grpAuxiliary );
            this.m_pnlCommands.Controls.Add( this.m_grpJog );
            this.m_pnlCommands.Controls.Add( this.m_grpHome );
            this.m_pnlCommands.Controls.Add( this.m_grpStrategy );
            this.m_pnlCommands.Controls.Add( this.m_grpMachining );
            this.m_pnlCommands.Enabled = false;
            this.m_pnlCommands.Location = new System.Drawing.Point( 1, 4 );
            this.m_pnlCommands.Margin = new System.Windows.Forms.Padding( 0 );
            this.m_pnlCommands.Name = "m_pnlCommands";
            this.m_pnlCommands.Size = new System.Drawing.Size( 216, 686 );
            this.m_pnlCommands.TabIndex = 0;
            //
            // m_grpAuxiliary
            //
            this.m_grpAuxiliary.Controls.Add( this.m_lblAuxiliaryTitle );
            this.m_grpAuxiliary.Controls.Add( this.m_btnOuterEquipParameters );
            this.m_grpAuxiliary.Controls.Add( this.m_btnShutter );
            this.m_grpAuxiliary.Controls.Add( this.m_btnAir );
            this.m_grpAuxiliary.Controls.Add( this.m_btnFollow );
            this.m_grpAuxiliary.Controls.Add( this.m_btnLaser );
            this.m_grpAuxiliary.Controls.Add( this.m_btnFocus );
            this.m_grpAuxiliary.Controls.Add( this.m_btnCalibration );
            this.m_grpAuxiliary.Location = new System.Drawing.Point( 0, 0 );
            this.m_grpAuxiliary.Margin = new System.Windows.Forms.Padding( 0 );
            this.m_grpAuxiliary.Name = "m_grpAuxiliary";
            this.m_grpAuxiliary.Size = new System.Drawing.Size( 216, 108 );
            this.m_grpAuxiliary.TabIndex = 0;
            //
            // m_lblAuxiliaryTitle
            //
            this.m_lblAuxiliaryTitle.Location = new System.Drawing.Point( 11, 4 );
            this.m_lblAuxiliaryTitle.Name = "m_lblAuxiliaryTitle";
            this.m_lblAuxiliaryTitle.Role = CommonUI.Theme.TextRole.Emphasis;
            this.m_lblAuxiliaryTitle.Size = new System.Drawing.Size( 120, 24 );
            this.m_lblAuxiliaryTitle.TabIndex = 0;
            this.m_lblAuxiliaryTitle.Text = "功能控制";
            this.m_lblAuxiliaryTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_btnOuterEquipParameters
            //
            this.m_btnOuterEquipParameters.Location = new System.Drawing.Point( 146, 4 );
            this.m_btnOuterEquipParameters.Name = "m_btnOuterEquipParameters";
            this.m_btnOuterEquipParameters.Size = new System.Drawing.Size( 66, 24 );
            this.m_btnOuterEquipParameters.TabIndex = 1;
            this.m_btnOuterEquipParameters.Text = "參數設置";
            //
            // m_btnShutter
            //
            this.m_btnShutter.Location = new System.Drawing.Point( 4, 34 );
            this.m_btnShutter.Name = "m_btnShutter";
            this.m_btnShutter.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnShutter.TabIndex = 2;
            this.m_btnShutter.Text = "光閘";
            //
            // m_btnAir
            //
            this.m_btnAir.Location = new System.Drawing.Point( 75, 34 );
            this.m_btnAir.Name = "m_btnAir";
            this.m_btnAir.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnAir.TabIndex = 3;
            this.m_btnAir.Text = "吹氣";
            //
            // m_btnFollow
            //
            this.m_btnFollow.Location = new System.Drawing.Point( 146, 34 );
            this.m_btnFollow.Name = "m_btnFollow";
            this.m_btnFollow.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnFollow.TabIndex = 4;
            this.m_btnFollow.Text = "隨動";
            //
            // m_btnLaser
            //
            this.m_btnLaser.Location = new System.Drawing.Point( 4, 71 );
            this.m_btnLaser.Name = "m_btnLaser";
            this.m_btnLaser.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnLaser.TabIndex = 5;
            this.m_btnLaser.Text = "雷射";
            //
            // m_btnFocus
            //
            this.m_btnFocus.Location = new System.Drawing.Point( 75, 71 );
            this.m_btnFocus.Name = "m_btnFocus";
            this.m_btnFocus.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnFocus.TabIndex = 6;
            this.m_btnFocus.Text = "調焦";
            //
            // m_btnCalibration
            //
            this.m_btnCalibration.Location = new System.Drawing.Point( 146, 71 );
            this.m_btnCalibration.Name = "m_btnCalibration";
            this.m_btnCalibration.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnCalibration.TabIndex = 7;
            this.m_btnCalibration.Text = "電容標定";
            //
            // m_grpJog
            //
            this.m_grpJog.Controls.Add( this.m_lblJogTitle );
            this.m_grpJog.Controls.Add( this.m_btnJogStep5 );
            this.m_grpJog.Controls.Add( this.m_btnJogStep10 );
            this.m_grpJog.Controls.Add( this.m_btnJogStep50 );
            this.m_grpJog.Controls.Add( this.m_btnJogStep100 );
            this.m_grpJog.Controls.Add( this.m_btnJogStep200 );
            this.m_grpJog.Controls.Add( this.m_btnZPlus );
            this.m_grpJog.Controls.Add( this.m_btnYPlus );
            this.m_grpJog.Controls.Add( this.m_btnBPlus );
            this.m_grpJog.Controls.Add( this.m_btnXMinus );
            this.m_grpJog.Controls.Add( this.m_btnJog );
            this.m_grpJog.Controls.Add( this.m_btnXPlus );
            this.m_grpJog.Controls.Add( this.m_btnZMinus );
            this.m_grpJog.Controls.Add( this.m_btnYMinus );
            this.m_grpJog.Controls.Add( this.m_btnBMinus );
            this.m_grpJog.Controls.Add( this.m_btnCMinus );
            this.m_grpJog.Controls.Add( this.m_btnEndpoint );
            this.m_grpJog.Controls.Add( this.m_btnCPlus );
            this.m_grpJog.Location = new System.Drawing.Point( 0, 116 );
            this.m_grpJog.Margin = new System.Windows.Forms.Padding( 0 );
            this.m_grpJog.Name = "m_grpJog";
            this.m_grpJog.Size = new System.Drawing.Size( 216, 234 );
            this.m_grpJog.TabIndex = 1;
            //
            // m_lblJogTitle
            //
            this.m_lblJogTitle.Location = new System.Drawing.Point( 11, 4 );
            this.m_lblJogTitle.Name = "m_lblJogTitle";
            this.m_lblJogTitle.Role = CommonUI.Theme.TextRole.Emphasis;
            this.m_lblJogTitle.Size = new System.Drawing.Size( 193, 22 );
            this.m_lblJogTitle.TabIndex = 0;
            this.m_lblJogTitle.Text = "軸向移動";
            this.m_lblJogTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_btnJogStep5
            //
            this.m_btnJogStep5.Location = new System.Drawing.Point( 4, 30 );
            this.m_btnJogStep5.Name = "m_btnJogStep5";
            this.m_btnJogStep5.Size = new System.Drawing.Size( 38, 28 );
            this.m_btnJogStep5.TabIndex = 1;
            this.m_btnJogStep5.Text = "0.05";
            //
            // m_btnJogStep10
            //
            this.m_btnJogStep10.Location = new System.Drawing.Point( 47, 30 );
            this.m_btnJogStep10.Name = "m_btnJogStep10";
            this.m_btnJogStep10.Size = new System.Drawing.Size( 37, 28 );
            this.m_btnJogStep10.TabIndex = 2;
            this.m_btnJogStep10.Text = "0.1";
            //
            // m_btnJogStep50
            //
            this.m_btnJogStep50.Location = new System.Drawing.Point( 89, 30 );
            this.m_btnJogStep50.Name = "m_btnJogStep50";
            this.m_btnJogStep50.Size = new System.Drawing.Size( 38, 28 );
            this.m_btnJogStep50.TabIndex = 3;
            this.m_btnJogStep50.Text = "0.5";
            //
            // m_btnJogStep100
            //
            this.m_btnJogStep100.Active = true;
            this.m_btnJogStep100.Location = new System.Drawing.Point( 132, 30 );
            this.m_btnJogStep100.Name = "m_btnJogStep100";
            this.m_btnJogStep100.Size = new System.Drawing.Size( 37, 28 );
            this.m_btnJogStep100.TabIndex = 4;
            this.m_btnJogStep100.Text = "1.0";
            //
            // m_btnJogStep200
            //
            this.m_btnJogStep200.Location = new System.Drawing.Point( 174, 30 );
            this.m_btnJogStep200.Name = "m_btnJogStep200";
            this.m_btnJogStep200.Size = new System.Drawing.Size( 38, 28 );
            this.m_btnJogStep200.TabIndex = 5;
            this.m_btnJogStep200.Text = "2.0";
            //
            // m_btnZPlus
            //
            this.m_btnZPlus.Location = new System.Drawing.Point( 4, 66 );
            this.m_btnZPlus.Name = "m_btnZPlus";
            this.m_btnZPlus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnZPlus.TabIndex = 6;
            this.m_btnZPlus.Text = "Z +";
            //
            // m_btnYPlus
            //
            this.m_btnYPlus.Location = new System.Drawing.Point( 75, 66 );
            this.m_btnYPlus.Name = "m_btnYPlus";
            this.m_btnYPlus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnYPlus.TabIndex = 7;
            this.m_btnYPlus.Text = "Y +";
            //
            // m_btnBPlus
            //
            this.m_btnBPlus.Location = new System.Drawing.Point( 146, 66 );
            this.m_btnBPlus.Name = "m_btnBPlus";
            this.m_btnBPlus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnBPlus.TabIndex = 8;
            this.m_btnBPlus.Text = "B +";
            //
            // m_btnXMinus
            //
            this.m_btnXMinus.Location = new System.Drawing.Point( 4, 108 );
            this.m_btnXMinus.Name = "m_btnXMinus";
            this.m_btnXMinus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnXMinus.TabIndex = 9;
            this.m_btnXMinus.Text = "X −";
            //
            // m_btnJog
            //
            this.m_btnJog.Location = new System.Drawing.Point( 75, 108 );
            this.m_btnJog.Name = "m_btnJog";
            this.m_btnJog.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnJog.TabIndex = 10;
            this.m_btnJog.Text = "點動";
            //
            // m_btnXPlus
            //
            this.m_btnXPlus.Location = new System.Drawing.Point( 146, 108 );
            this.m_btnXPlus.Name = "m_btnXPlus";
            this.m_btnXPlus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnXPlus.TabIndex = 11;
            this.m_btnXPlus.Text = "X +";
            //
            // m_btnZMinus
            //
            this.m_btnZMinus.Location = new System.Drawing.Point( 4, 150 );
            this.m_btnZMinus.Name = "m_btnZMinus";
            this.m_btnZMinus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnZMinus.TabIndex = 12;
            this.m_btnZMinus.Text = "Z −";
            //
            // m_btnYMinus
            //
            this.m_btnYMinus.Location = new System.Drawing.Point( 75, 150 );
            this.m_btnYMinus.Name = "m_btnYMinus";
            this.m_btnYMinus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnYMinus.TabIndex = 13;
            this.m_btnYMinus.Text = "Y −";
            //
            // m_btnBMinus
            //
            this.m_btnBMinus.Location = new System.Drawing.Point( 146, 150 );
            this.m_btnBMinus.Name = "m_btnBMinus";
            this.m_btnBMinus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnBMinus.TabIndex = 14;
            this.m_btnBMinus.Text = "B −";
            //
            // m_btnCMinus
            //
            this.m_btnCMinus.Location = new System.Drawing.Point( 4, 192 );
            this.m_btnCMinus.Name = "m_btnCMinus";
            this.m_btnCMinus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnCMinus.TabIndex = 15;
            this.m_btnCMinus.Text = "C −";
            //
            // m_btnEndpoint
            //
            this.m_btnEndpoint.Location = new System.Drawing.Point( 75, 192 );
            this.m_btnEndpoint.Name = "m_btnEndpoint";
            this.m_btnEndpoint.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnEndpoint.TabIndex = 16;
            this.m_btnEndpoint.Text = "控端點";
            //
            // m_btnCPlus
            //
            this.m_btnCPlus.Location = new System.Drawing.Point( 146, 192 );
            this.m_btnCPlus.Name = "m_btnCPlus";
            this.m_btnCPlus.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnCPlus.TabIndex = 17;
            this.m_btnCPlus.Text = "C +";
            //
            // m_grpHome
            //
            this.m_grpHome.Controls.Add( this.m_lblHomeTitle );
            this.m_grpHome.Controls.Add( this.m_btnHomeProgram );
            this.m_grpHome.Controls.Add( this.m_btnHomeMachine );
            this.m_grpHome.Controls.Add( this.m_btnSafePosition );
            this.m_grpHome.Location = new System.Drawing.Point( 0, 358 );
            this.m_grpHome.Margin = new System.Windows.Forms.Padding( 0 );
            this.m_grpHome.Name = "m_grpHome";
            this.m_grpHome.Size = new System.Drawing.Size( 216, 72 );
            this.m_grpHome.TabIndex = 2;
            //
            // m_lblHomeTitle
            //
            this.m_lblHomeTitle.Location = new System.Drawing.Point( 11, 4 );
            this.m_lblHomeTitle.Name = "m_lblHomeTitle";
            this.m_lblHomeTitle.Role = CommonUI.Theme.TextRole.Emphasis;
            this.m_lblHomeTitle.Size = new System.Drawing.Size( 193, 22 );
            this.m_lblHomeTitle.TabIndex = 0;
            this.m_lblHomeTitle.Text = "回零";
            this.m_lblHomeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_btnHomeProgram
            //
            this.m_btnHomeProgram.Location = new System.Drawing.Point( 4, 30 );
            this.m_btnHomeProgram.Name = "m_btnHomeProgram";
            this.m_btnHomeProgram.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnHomeProgram.TabIndex = 1;
            this.m_btnHomeProgram.Text = "回程式零";
            //
            // m_btnHomeMachine
            //
            this.m_btnHomeMachine.Location = new System.Drawing.Point( 75, 30 );
            this.m_btnHomeMachine.Name = "m_btnHomeMachine";
            this.m_btnHomeMachine.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnHomeMachine.TabIndex = 2;
            this.m_btnHomeMachine.Text = "回機械零";
            //
            // m_btnSafePosition
            //
            this.m_btnSafePosition.Location = new System.Drawing.Point( 146, 30 );
            this.m_btnSafePosition.Name = "m_btnSafePosition";
            this.m_btnSafePosition.Size = new System.Drawing.Size( 66, 36 );
            this.m_btnSafePosition.TabIndex = 3;
            this.m_btnSafePosition.Text = "回安全位置";
            //
            // m_grpStrategy
            //
            this.m_grpStrategy.Controls.Add( this.m_lblStrategyTitle );
            this.m_grpStrategy.Controls.Add( this.m_btnResumeCutParameters );
            this.m_grpStrategy.Controls.Add( this.m_btnDryRun );
            this.m_grpStrategy.Controls.Add( this.m_btnResumeCutting );
            this.m_grpStrategy.Controls.Add( this.m_btnHandwheelSimulation );
            this.m_grpStrategy.Location = new System.Drawing.Point( 0, 438 );
            this.m_grpStrategy.Margin = new System.Windows.Forms.Padding( 0 );
            this.m_grpStrategy.Name = "m_grpStrategy";
            this.m_grpStrategy.Size = new System.Drawing.Size( 216, 72 );
            this.m_grpStrategy.TabIndex = 3;
            //
            // m_lblStrategyTitle
            //
            this.m_lblStrategyTitle.Location = new System.Drawing.Point( 11, 4 );
            this.m_lblStrategyTitle.Name = "m_lblStrategyTitle";
            this.m_lblStrategyTitle.Role = CommonUI.Theme.TextRole.Emphasis;
            this.m_lblStrategyTitle.Size = new System.Drawing.Size( 120, 24 );
            this.m_lblStrategyTitle.TabIndex = 0;
            this.m_lblStrategyTitle.Text = "加工策略";
            this.m_lblStrategyTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_btnResumeCutParameters
            //
            this.m_btnResumeCutParameters.Location = new System.Drawing.Point( 146, 4 );
            this.m_btnResumeCutParameters.Name = "m_btnResumeCutParameters";
            this.m_btnResumeCutParameters.Size = new System.Drawing.Size( 66, 24 );
            this.m_btnResumeCutParameters.TabIndex = 1;
            this.m_btnResumeCutParameters.Text = "參數設置";
            //
            // m_btnDryRun
            //
            this.m_btnDryRun.Location = new System.Drawing.Point( 4, 34 );
            this.m_btnDryRun.Name = "m_btnDryRun";
            this.m_btnDryRun.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnDryRun.TabIndex = 2;
            this.m_btnDryRun.Text = "空走";
            //
            // m_btnResumeCutting
            //
            this.m_btnResumeCutting.Location = new System.Drawing.Point( 75, 34 );
            this.m_btnResumeCutting.Name = "m_btnResumeCutting";
            this.m_btnResumeCutting.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnResumeCutting.TabIndex = 3;
            this.m_btnResumeCutting.Text = "斷點續切";
            //
            // m_btnHandwheelSimulation
            //
            this.m_btnHandwheelSimulation.Location = new System.Drawing.Point( 146, 34 );
            this.m_btnHandwheelSimulation.Name = "m_btnHandwheelSimulation";
            this.m_btnHandwheelSimulation.Size = new System.Drawing.Size( 66, 32 );
            this.m_btnHandwheelSimulation.TabIndex = 4;
            this.m_btnHandwheelSimulation.Text = "手輪模擬";
            //
            // m_grpMachining
            //
            this.m_grpMachining.Controls.Add( this.m_lblMachiningTitle );
            this.m_grpMachining.Controls.Add( this.m_chbThreePointCalibration );
            this.m_grpMachining.Controls.Add( this.m_lblFeedOverrideTitle );
            this.m_grpMachining.Controls.Add( this.m_lblFeedOverrideValue );
            this.m_grpMachining.Controls.Add( this.m_trbFeedOverride );
            this.m_grpMachining.Controls.Add( this.m_btnStart );
            this.m_grpMachining.Controls.Add( this.m_btnPause );
            this.m_grpMachining.Controls.Add( this.m_btnContinue );
            this.m_grpMachining.Controls.Add( this.m_btnStop );
            this.m_grpMachining.Location = new System.Drawing.Point( 0, 518 );
            this.m_grpMachining.Margin = new System.Windows.Forms.Padding( 0 );
            this.m_grpMachining.Name = "m_grpMachining";
            this.m_grpMachining.Size = new System.Drawing.Size( 216, 168 );
            this.m_grpMachining.TabIndex = 4;
            //
            // m_lblMachiningTitle
            //
            this.m_lblMachiningTitle.Location = new System.Drawing.Point( 11, 4 );
            this.m_lblMachiningTitle.Name = "m_lblMachiningTitle";
            this.m_lblMachiningTitle.Role = CommonUI.Theme.TextRole.Emphasis;
            this.m_lblMachiningTitle.Size = new System.Drawing.Size( 97, 22 );
            this.m_lblMachiningTitle.TabIndex = 0;
            this.m_lblMachiningTitle.Text = "加工控制";
            this.m_lblMachiningTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_chbThreePointCalibration
            //
            this.m_chbThreePointCalibration.Location = new System.Drawing.Point( 118, 4 );
            this.m_chbThreePointCalibration.Name = "m_chbThreePointCalibration";
            this.m_chbThreePointCalibration.Size = new System.Drawing.Size( 90, 22 );
            this.m_chbThreePointCalibration.TabIndex = 1;
            this.m_chbThreePointCalibration.Text = "三點校正";
            //
            // m_lblFeedOverrideTitle
            //
            this.m_lblFeedOverrideTitle.Location = new System.Drawing.Point( 4, 28 );
            this.m_lblFeedOverrideTitle.Name = "m_lblFeedOverrideTitle";
            this.m_lblFeedOverrideTitle.Size = new System.Drawing.Size( 110, 22 );
            this.m_lblFeedOverrideTitle.TabIndex = 2;
            this.m_lblFeedOverrideTitle.Text = "進給倍率";
            this.m_lblFeedOverrideTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_lblFeedOverrideValue
            //
            this.m_lblFeedOverrideValue.Location = new System.Drawing.Point( 118, 28 );
            this.m_lblFeedOverrideValue.Name = "m_lblFeedOverrideValue";
            this.m_lblFeedOverrideValue.Role = CommonUI.Theme.TextRole.Value;
            this.m_lblFeedOverrideValue.Size = new System.Drawing.Size( 94, 22 );
            this.m_lblFeedOverrideValue.TabIndex = 3;
            this.m_lblFeedOverrideValue.Text = "[100]%";
            this.m_lblFeedOverrideValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // m_trbFeedOverride
            //
            this.m_trbFeedOverride.AutoSize = false;
            this.m_trbFeedOverride.LargeChange = 10;
            this.m_trbFeedOverride.Location = new System.Drawing.Point( 4, 50 );
            this.m_trbFeedOverride.Maximum = 200;
            this.m_trbFeedOverride.Name = "m_trbFeedOverride";
            this.m_trbFeedOverride.Size = new System.Drawing.Size( 208, 28 );
            this.m_trbFeedOverride.TabIndex = 4;
            this.m_trbFeedOverride.TickStyle = System.Windows.Forms.TickStyle.None;
            this.m_trbFeedOverride.Value = 100;
            //
            // m_btnStart
            //
            this.m_btnStart.Location = new System.Drawing.Point( 4, 82 );
            this.m_btnStart.Name = "m_btnStart";
            this.m_btnStart.Kind = CommonUI.Controls.ButtonKind.Primary;
            this.m_btnStart.Size = new System.Drawing.Size( 101, 38 );
            this.m_btnStart.TabIndex = 5;
            this.m_btnStart.Text = "開始";
            //
            // m_btnPause
            //
            this.m_btnPause.Location = new System.Drawing.Point( 111, 82 );
            this.m_btnPause.Name = "m_btnPause";
            this.m_btnPause.Size = new System.Drawing.Size( 101, 38 );
            this.m_btnPause.TabIndex = 6;
            this.m_btnPause.Text = "暫停";
            //
            // m_btnContinue
            //
            this.m_btnContinue.Location = new System.Drawing.Point( 4, 126 );
            this.m_btnContinue.Name = "m_btnContinue";
            this.m_btnContinue.Size = new System.Drawing.Size( 101, 38 );
            this.m_btnContinue.TabIndex = 7;
            this.m_btnContinue.Text = "繼續";
            //
            // m_btnStop
            //
            this.m_btnStop.Location = new System.Drawing.Point( 111, 126 );
            this.m_btnStop.Name = "m_btnStop";
            this.m_btnStop.Kind = CommonUI.Controls.ButtonKind.Danger;
            this.m_btnStop.Size = new System.Drawing.Size( 101, 38 );
            this.m_btnStop.TabIndex = 8;
            this.m_btnStop.Text = "停止";
            //
            // CNCPanel
            //
            this.AutoScroll = false;
            this.Controls.Add( this.m_pnlCommands );
            this.Name = "CNCPanel";
            this.Padding = new System.Windows.Forms.Padding( 1, 4, 1, 4 );
            this.Size = new System.Drawing.Size( 240, 700 );
            this.m_pnlCommands.ResumeLayout( false );
            this.m_grpAuxiliary.ResumeLayout( false );
            this.m_grpJog.ResumeLayout( false );
            this.m_grpHome.ResumeLayout( false );
            this.m_grpStrategy.ResumeLayout( false );
            this.m_grpMachining.ResumeLayout( false );
            ( (System.ComponentModel.ISupportInitialize)( this.m_trbFeedOverride ) ).EndInit();
            this.ResumeLayout( false );
        }

        #endregion

        private System.Windows.Forms.Panel m_pnlCommands;
        private CommonUI.Controls.SectionPanel m_grpAuxiliary;
        private CommonUI.Controls.ThemedLabel m_lblAuxiliaryTitle;
        private CommonUI.Controls.ThemedButton m_btnOuterEquipParameters;
        private CommonUI.Controls.ThemedButton m_btnShutter;
        private CommonUI.Controls.ThemedButton m_btnAir;
        private CommonUI.Controls.ThemedButton m_btnFollow;
        private CommonUI.Controls.ThemedButton m_btnLaser;
        private CommonUI.Controls.ThemedButton m_btnFocus;
        private CommonUI.Controls.ThemedButton m_btnCalibration;
        private CommonUI.Controls.SectionPanel m_grpJog;
        private CommonUI.Controls.ThemedLabel m_lblJogTitle;
        private CommonUI.Controls.ThemedButton m_btnJogStep5;
        private CommonUI.Controls.ThemedButton m_btnJogStep10;
        private CommonUI.Controls.ThemedButton m_btnJogStep50;
        private CommonUI.Controls.ThemedButton m_btnJogStep100;
        private CommonUI.Controls.ThemedButton m_btnJogStep200;
        private CommonUI.Controls.ThemedButton m_btnZPlus;
        private CommonUI.Controls.ThemedButton m_btnYPlus;
        private CommonUI.Controls.ThemedButton m_btnBPlus;
        private CommonUI.Controls.ThemedButton m_btnXMinus;
        private CommonUI.Controls.ThemedButton m_btnJog;
        private CommonUI.Controls.ThemedButton m_btnXPlus;
        private CommonUI.Controls.ThemedButton m_btnZMinus;
        private CommonUI.Controls.ThemedButton m_btnYMinus;
        private CommonUI.Controls.ThemedButton m_btnBMinus;
        private CommonUI.Controls.ThemedButton m_btnCMinus;
        private CommonUI.Controls.ThemedButton m_btnEndpoint;
        private CommonUI.Controls.ThemedButton m_btnCPlus;
        private CommonUI.Controls.SectionPanel m_grpHome;
        private CommonUI.Controls.ThemedLabel m_lblHomeTitle;
        private CommonUI.Controls.ThemedButton m_btnHomeProgram;
        private CommonUI.Controls.ThemedButton m_btnHomeMachine;
        private CommonUI.Controls.ThemedButton m_btnSafePosition;
        private CommonUI.Controls.SectionPanel m_grpStrategy;
        private CommonUI.Controls.ThemedLabel m_lblStrategyTitle;
        private CommonUI.Controls.ThemedButton m_btnResumeCutParameters;
        private CommonUI.Controls.ThemedButton m_btnDryRun;
        private CommonUI.Controls.ThemedButton m_btnResumeCutting;
        private CommonUI.Controls.ThemedButton m_btnHandwheelSimulation;
        private CommonUI.Controls.SectionPanel m_grpMachining;
        private CommonUI.Controls.ThemedLabel m_lblMachiningTitle;
        private System.Windows.Forms.CheckBox m_chbThreePointCalibration;
        private CommonUI.Controls.ThemedLabel m_lblFeedOverrideTitle;
        private CommonUI.Controls.ThemedLabel m_lblFeedOverrideValue;
        private System.Windows.Forms.TrackBar m_trbFeedOverride;
        private CommonUI.Controls.ThemedButton m_btnStart;
        private CommonUI.Controls.ThemedButton m_btnPause;
        private CommonUI.Controls.ThemedButton m_btnContinue;
        private CommonUI.Controls.ThemedButton m_btnStop;
    }
}
