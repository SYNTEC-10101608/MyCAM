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
			this.m_pnlAuxiliary = new CommonUI.Controls.SectionPanel();
			this.m_btnOuterEquipParameters = new CommonUI.Controls.ThemedButton();
			this.m_btnShutter = new CommonUI.Controls.ThemedButton();
			this.m_btnAir = new CommonUI.Controls.ThemedButton();
			this.m_btnFollow = new CommonUI.Controls.ThemedButton();
			this.m_btnLaser = new CommonUI.Controls.ThemedButton();
			this.m_btnFocus = new CommonUI.Controls.ThemedButton();
			this.m_btnCalibration = new CommonUI.Controls.ThemedButton();
			this.m_pnlJog = new CommonUI.Controls.SectionPanel();
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
			this.m_pnlHome = new CommonUI.Controls.SectionPanel();
			this.m_btnHomeProgram = new CommonUI.Controls.ThemedButton();
			this.m_btnHomeMachine = new CommonUI.Controls.ThemedButton();
			this.m_btnSafePosition = new CommonUI.Controls.ThemedButton();
			this.m_pnlStrategy = new CommonUI.Controls.SectionPanel();
			this.m_btnResumeCutParameters = new CommonUI.Controls.ThemedButton();
			this.m_btnDryRun = new CommonUI.Controls.ThemedButton();
			this.m_btnResumeCutting = new CommonUI.Controls.ThemedButton();
			this.m_btnHandwheelSimulation = new CommonUI.Controls.ThemedButton();
			this.m_pnlMachining = new CommonUI.Controls.SectionPanel();
			this.m_chkThreePointCalibration = new System.Windows.Forms.CheckBox();
			this.m_lblFeedOverrideTitle = new CommonUI.Controls.ThemedLabel();
			this.m_lblFeedOverrideValue = new CommonUI.Controls.ThemedLabel();
			this.m_trbFeedOverride = new System.Windows.Forms.TrackBar();
			this.m_btnStart = new CommonUI.Controls.ThemedButton();
			this.m_btnPause = new CommonUI.Controls.ThemedButton();
			this.m_btnContinue = new CommonUI.Controls.ThemedButton();
			this.m_btnStop = new CommonUI.Controls.ThemedButton();
			this.m_pnlCommands.SuspendLayout();
			this.m_pnlAuxiliary.SuspendLayout();
			this.m_pnlJog.SuspendLayout();
			this.m_pnlHome.SuspendLayout();
			this.m_pnlStrategy.SuspendLayout();
			this.m_pnlMachining.SuspendLayout();
			( (System.ComponentModel.ISupportInitialize)( this.m_trbFeedOverride ) ).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlCommands
			//
			this.m_pnlCommands.Controls.Add( this.m_pnlAuxiliary );
			this.m_pnlCommands.Controls.Add( this.m_pnlJog );
			this.m_pnlCommands.Controls.Add( this.m_pnlHome );
			this.m_pnlCommands.Controls.Add( this.m_pnlStrategy );
			this.m_pnlCommands.Controls.Add( this.m_pnlMachining );
			this.m_pnlCommands.Enabled = false;
			this.m_pnlCommands.Location = new System.Drawing.Point(8, 8);
			this.m_pnlCommands.Margin = new System.Windows.Forms.Padding( 0 );
			this.m_pnlCommands.Name = "m_pnlCommands";
			this.m_pnlCommands.Size = new System.Drawing.Size(272, 740);
			this.m_pnlCommands.TabIndex = 0;
			//
			// m_pnlAuxiliary
			//
			this.m_pnlAuxiliary.Controls.Add( this.m_btnOuterEquipParameters );
			this.m_pnlAuxiliary.Controls.Add( this.m_btnShutter );
			this.m_pnlAuxiliary.Controls.Add( this.m_btnAir );
			this.m_pnlAuxiliary.Controls.Add( this.m_btnFollow );
			this.m_pnlAuxiliary.Controls.Add( this.m_btnLaser );
			this.m_pnlAuxiliary.Controls.Add( this.m_btnFocus );
			this.m_pnlAuxiliary.Controls.Add( this.m_btnCalibration );
			this.m_pnlAuxiliary.Location = new System.Drawing.Point(0, 0);
			this.m_pnlAuxiliary.Margin = new System.Windows.Forms.Padding( 0 );
			this.m_pnlAuxiliary.Name = "m_pnlAuxiliary";
			this.m_pnlAuxiliary.Size = new System.Drawing.Size(272, 116);
			this.m_pnlAuxiliary.TabIndex = 0;
			this.m_pnlAuxiliary.Text = "功能控制";
			//
			// m_btnOuterEquipParameters
			//
			this.m_btnOuterEquipParameters.Location = new System.Drawing.Point(182, 5);
			this.m_btnOuterEquipParameters.Name = "m_btnOuterEquipParameters";
			this.m_btnOuterEquipParameters.Size = new System.Drawing.Size(80, 23);
			this.m_btnOuterEquipParameters.TabIndex = 0;
			this.m_btnOuterEquipParameters.Text = "參數設置";
			//
			// m_btnShutter
			//
			this.m_btnShutter.Location = new System.Drawing.Point(10, 36);
			this.m_btnShutter.Name = "m_btnShutter";
			this.m_btnShutter.Size = new System.Drawing.Size(80, 32);
			this.m_btnShutter.TabIndex = 1;
			this.m_btnShutter.Text = "光閘";
			//
			// m_btnAir
			//
			this.m_btnAir.Location = new System.Drawing.Point(96, 36);
			this.m_btnAir.Name = "m_btnAir";
			this.m_btnAir.Size = new System.Drawing.Size(80, 32);
			this.m_btnAir.TabIndex = 2;
			this.m_btnAir.Text = "吹氣";
			//
			// m_btnFollow
			//
			this.m_btnFollow.Location = new System.Drawing.Point(182, 36);
			this.m_btnFollow.Name = "m_btnFollow";
			this.m_btnFollow.Size = new System.Drawing.Size(80, 32);
			this.m_btnFollow.TabIndex = 3;
			this.m_btnFollow.Text = "隨動";
			//
			// m_btnLaser
			//
			this.m_btnLaser.Location = new System.Drawing.Point(10, 74);
			this.m_btnLaser.Name = "m_btnLaser";
			this.m_btnLaser.Size = new System.Drawing.Size(80, 32);
			this.m_btnLaser.TabIndex = 4;
			this.m_btnLaser.Text = "雷射";
			//
			// m_btnFocus
			//
			this.m_btnFocus.Location = new System.Drawing.Point(96, 74);
			this.m_btnFocus.Name = "m_btnFocus";
			this.m_btnFocus.Size = new System.Drawing.Size(80, 32);
			this.m_btnFocus.TabIndex = 5;
			this.m_btnFocus.Text = "調焦";
			//
			// m_btnCalibration
			//
			this.m_btnCalibration.Location = new System.Drawing.Point(182, 74);
			this.m_btnCalibration.Name = "m_btnCalibration";
			this.m_btnCalibration.Size = new System.Drawing.Size(80, 32);
			this.m_btnCalibration.TabIndex = 6;
			this.m_btnCalibration.Text = "電容標定";
			//
			// m_pnlJog
			//
			this.m_pnlJog.Controls.Add( this.m_btnJogStep5 );
			this.m_pnlJog.Controls.Add( this.m_btnJogStep10 );
			this.m_pnlJog.Controls.Add( this.m_btnJogStep50 );
			this.m_pnlJog.Controls.Add( this.m_btnJogStep100 );
			this.m_pnlJog.Controls.Add( this.m_btnJogStep200 );
			this.m_pnlJog.Controls.Add( this.m_btnZPlus );
			this.m_pnlJog.Controls.Add( this.m_btnYPlus );
			this.m_pnlJog.Controls.Add( this.m_btnBPlus );
			this.m_pnlJog.Controls.Add( this.m_btnXMinus );
			this.m_pnlJog.Controls.Add( this.m_btnJog );
			this.m_pnlJog.Controls.Add( this.m_btnXPlus );
			this.m_pnlJog.Controls.Add( this.m_btnZMinus );
			this.m_pnlJog.Controls.Add( this.m_btnYMinus );
			this.m_pnlJog.Controls.Add( this.m_btnBMinus );
			this.m_pnlJog.Controls.Add( this.m_btnCMinus );
			this.m_pnlJog.Controls.Add( this.m_btnEndpoint );
			this.m_pnlJog.Controls.Add( this.m_btnCPlus );
			this.m_pnlJog.Location = new System.Drawing.Point(0, 124);
			this.m_pnlJog.Margin = new System.Windows.Forms.Padding( 0 );
			this.m_pnlJog.Name = "m_pnlJog";
			this.m_pnlJog.Size = new System.Drawing.Size(272, 242);
			this.m_pnlJog.TabIndex = 1;
			this.m_pnlJog.Text = "軸向移動";
			//
			// m_btnJogStep5
			//
			this.m_btnJogStep5.Location = new System.Drawing.Point(10, 36);
			this.m_btnJogStep5.Name = "m_btnJogStep5";
			this.m_btnJogStep5.Size = new System.Drawing.Size(46, 28);
			this.m_btnJogStep5.TabIndex = 0;
			this.m_btnJogStep5.Text = "0.05";
			//
			// m_btnJogStep10
			//
			this.m_btnJogStep10.Location = new System.Drawing.Point(62, 36);
			this.m_btnJogStep10.Name = "m_btnJogStep10";
			this.m_btnJogStep10.Size = new System.Drawing.Size(45, 28);
			this.m_btnJogStep10.TabIndex = 1;
			this.m_btnJogStep10.Text = "0.1";
			//
			// m_btnJogStep50
			//
			this.m_btnJogStep50.Location = new System.Drawing.Point(113, 36);
			this.m_btnJogStep50.Name = "m_btnJogStep50";
			this.m_btnJogStep50.Size = new System.Drawing.Size(46, 28);
			this.m_btnJogStep50.TabIndex = 2;
			this.m_btnJogStep50.Text = "0.5";
			//
			// m_btnJogStep100
			//
			this.m_btnJogStep100.Active = true;
			this.m_btnJogStep100.Location = new System.Drawing.Point(165, 36);
			this.m_btnJogStep100.Name = "m_btnJogStep100";
			this.m_btnJogStep100.Size = new System.Drawing.Size(45, 28);
			this.m_btnJogStep100.TabIndex = 3;
			this.m_btnJogStep100.Text = "1.0";
			//
			// m_btnJogStep200
			//
			this.m_btnJogStep200.Location = new System.Drawing.Point(216, 36);
			this.m_btnJogStep200.Name = "m_btnJogStep200";
			this.m_btnJogStep200.Size = new System.Drawing.Size(46, 28);
			this.m_btnJogStep200.TabIndex = 4;
			this.m_btnJogStep200.Text = "2.0";
			//
			// m_btnZPlus
			//
			this.m_btnZPlus.Location = new System.Drawing.Point(10, 70);
			this.m_btnZPlus.Name = "m_btnZPlus";
			this.m_btnZPlus.Size = new System.Drawing.Size(80, 36);
			this.m_btnZPlus.TabIndex = 5;
			this.m_btnZPlus.Text = "Z +";
			//
			// m_btnYPlus
			//
			this.m_btnYPlus.Location = new System.Drawing.Point(96, 70);
			this.m_btnYPlus.Name = "m_btnYPlus";
			this.m_btnYPlus.Size = new System.Drawing.Size(80, 36);
			this.m_btnYPlus.TabIndex = 6;
			this.m_btnYPlus.Text = "Y +";
			//
			// m_btnBPlus
			//
			this.m_btnBPlus.Location = new System.Drawing.Point(182, 70);
			this.m_btnBPlus.Name = "m_btnBPlus";
			this.m_btnBPlus.Size = new System.Drawing.Size(80, 36);
			this.m_btnBPlus.TabIndex = 7;
			this.m_btnBPlus.Text = "B +";
			//
			// m_btnXMinus
			//
			this.m_btnXMinus.Location = new System.Drawing.Point(10, 112);
			this.m_btnXMinus.Name = "m_btnXMinus";
			this.m_btnXMinus.Size = new System.Drawing.Size(80, 36);
			this.m_btnXMinus.TabIndex = 8;
			this.m_btnXMinus.Text = "X −";
			//
			// m_btnJog
			//
			this.m_btnJog.Location = new System.Drawing.Point(96, 112);
			this.m_btnJog.Name = "m_btnJog";
			this.m_btnJog.Size = new System.Drawing.Size(80, 36);
			this.m_btnJog.TabIndex = 9;
			this.m_btnJog.Text = "點動";
			//
			// m_btnXPlus
			//
			this.m_btnXPlus.Location = new System.Drawing.Point(182, 112);
			this.m_btnXPlus.Name = "m_btnXPlus";
			this.m_btnXPlus.Size = new System.Drawing.Size(80, 36);
			this.m_btnXPlus.TabIndex = 10;
			this.m_btnXPlus.Text = "X +";
			//
			// m_btnZMinus
			//
			this.m_btnZMinus.Location = new System.Drawing.Point(10, 154);
			this.m_btnZMinus.Name = "m_btnZMinus";
			this.m_btnZMinus.Size = new System.Drawing.Size(80, 36);
			this.m_btnZMinus.TabIndex = 11;
			this.m_btnZMinus.Text = "Z −";
			//
			// m_btnYMinus
			//
			this.m_btnYMinus.Location = new System.Drawing.Point(96, 154);
			this.m_btnYMinus.Name = "m_btnYMinus";
			this.m_btnYMinus.Size = new System.Drawing.Size(80, 36);
			this.m_btnYMinus.TabIndex = 12;
			this.m_btnYMinus.Text = "Y −";
			//
			// m_btnBMinus
			//
			this.m_btnBMinus.Location = new System.Drawing.Point(182, 154);
			this.m_btnBMinus.Name = "m_btnBMinus";
			this.m_btnBMinus.Size = new System.Drawing.Size(80, 36);
			this.m_btnBMinus.TabIndex = 13;
			this.m_btnBMinus.Text = "B −";
			//
			// m_btnCMinus
			//
			this.m_btnCMinus.Location = new System.Drawing.Point(10, 196);
			this.m_btnCMinus.Name = "m_btnCMinus";
			this.m_btnCMinus.Size = new System.Drawing.Size(80, 36);
			this.m_btnCMinus.TabIndex = 14;
			this.m_btnCMinus.Text = "C −";
			//
			// m_btnEndpoint
			//
			this.m_btnEndpoint.Location = new System.Drawing.Point(96, 196);
			this.m_btnEndpoint.Name = "m_btnEndpoint";
			this.m_btnEndpoint.Size = new System.Drawing.Size(80, 36);
			this.m_btnEndpoint.TabIndex = 15;
			this.m_btnEndpoint.Text = "控端點";
			//
			// m_btnCPlus
			//
			this.m_btnCPlus.Location = new System.Drawing.Point(182, 196);
			this.m_btnCPlus.Name = "m_btnCPlus";
			this.m_btnCPlus.Size = new System.Drawing.Size(80, 36);
			this.m_btnCPlus.TabIndex = 16;
			this.m_btnCPlus.Text = "C +";
			//
			// m_pnlHome
			//
			this.m_pnlHome.Controls.Add( this.m_btnHomeProgram );
			this.m_pnlHome.Controls.Add( this.m_btnHomeMachine );
			this.m_pnlHome.Controls.Add( this.m_btnSafePosition );
			this.m_pnlHome.Location = new System.Drawing.Point(0, 374);
			this.m_pnlHome.Margin = new System.Windows.Forms.Padding( 0 );
			this.m_pnlHome.Name = "m_pnlHome";
			this.m_pnlHome.Size = new System.Drawing.Size(272, 82);
			this.m_pnlHome.TabIndex = 2;
			this.m_pnlHome.Text = "回零";
			//
			// m_btnHomeProgram
			//
			this.m_btnHomeProgram.Location = new System.Drawing.Point(10, 36);
			this.m_btnHomeProgram.Name = "m_btnHomeProgram";
			this.m_btnHomeProgram.Size = new System.Drawing.Size(80, 36);
			this.m_btnHomeProgram.TabIndex = 0;
			this.m_btnHomeProgram.Text = "回程式零";
			//
			// m_btnHomeMachine
			//
			this.m_btnHomeMachine.Location = new System.Drawing.Point(96, 36);
			this.m_btnHomeMachine.Name = "m_btnHomeMachine";
			this.m_btnHomeMachine.Size = new System.Drawing.Size(80, 36);
			this.m_btnHomeMachine.TabIndex = 1;
			this.m_btnHomeMachine.Text = "回機械零";
			//
			// m_btnSafePosition
			//
			this.m_btnSafePosition.Location = new System.Drawing.Point(182, 36);
			this.m_btnSafePosition.Name = "m_btnSafePosition";
			this.m_btnSafePosition.Size = new System.Drawing.Size(80, 36);
			this.m_btnSafePosition.TabIndex = 2;
			this.m_btnSafePosition.Text = "回安全位置";
			//
			// m_pnlStrategy
			//
			this.m_pnlStrategy.Controls.Add( this.m_btnResumeCutParameters );
			this.m_pnlStrategy.Controls.Add( this.m_btnDryRun );
			this.m_pnlStrategy.Controls.Add( this.m_btnResumeCutting );
			this.m_pnlStrategy.Controls.Add( this.m_btnHandwheelSimulation );
			this.m_pnlStrategy.Location = new System.Drawing.Point(0, 464);
			this.m_pnlStrategy.Margin = new System.Windows.Forms.Padding( 0 );
			this.m_pnlStrategy.Name = "m_pnlStrategy";
			this.m_pnlStrategy.Size = new System.Drawing.Size(272, 78);
			this.m_pnlStrategy.TabIndex = 3;
			this.m_pnlStrategy.Text = "加工策略";
			//
			// m_btnResumeCutParameters
			//
			this.m_btnResumeCutParameters.Location = new System.Drawing.Point(182, 5);
			this.m_btnResumeCutParameters.Name = "m_btnResumeCutParameters";
			this.m_btnResumeCutParameters.Size = new System.Drawing.Size(80, 23);
			this.m_btnResumeCutParameters.TabIndex = 0;
			this.m_btnResumeCutParameters.Text = "參數設置";
			//
			// m_btnDryRun
			//
			this.m_btnDryRun.Location = new System.Drawing.Point(10, 36);
			this.m_btnDryRun.Name = "m_btnDryRun";
			this.m_btnDryRun.Size = new System.Drawing.Size(80, 32);
			this.m_btnDryRun.TabIndex = 1;
			this.m_btnDryRun.Text = "空走";
			//
			// m_btnResumeCutting
			//
			this.m_btnResumeCutting.Location = new System.Drawing.Point(96, 36);
			this.m_btnResumeCutting.Name = "m_btnResumeCutting";
			this.m_btnResumeCutting.Size = new System.Drawing.Size(80, 32);
			this.m_btnResumeCutting.TabIndex = 2;
			this.m_btnResumeCutting.Text = "斷點續切";
			//
			// m_btnHandwheelSimulation
			//
			this.m_btnHandwheelSimulation.Location = new System.Drawing.Point(182, 36);
			this.m_btnHandwheelSimulation.Name = "m_btnHandwheelSimulation";
			this.m_btnHandwheelSimulation.Size = new System.Drawing.Size(80, 32);
			this.m_btnHandwheelSimulation.TabIndex = 3;
			this.m_btnHandwheelSimulation.Text = "手輪模擬";
			//
			// m_pnlMachining
			//
			this.m_pnlMachining.Controls.Add( this.m_chkThreePointCalibration );
			this.m_pnlMachining.Controls.Add( this.m_lblFeedOverrideTitle );
			this.m_pnlMachining.Controls.Add( this.m_lblFeedOverrideValue );
			this.m_pnlMachining.Controls.Add( this.m_trbFeedOverride );
			this.m_pnlMachining.Controls.Add( this.m_btnStart );
			this.m_pnlMachining.Controls.Add( this.m_btnPause );
			this.m_pnlMachining.Controls.Add( this.m_btnContinue );
			this.m_pnlMachining.Controls.Add( this.m_btnStop );
			this.m_pnlMachining.Location = new System.Drawing.Point(0, 550);
			this.m_pnlMachining.Margin = new System.Windows.Forms.Padding( 0 );
			this.m_pnlMachining.Name = "m_pnlMachining";
			this.m_pnlMachining.Size = new System.Drawing.Size(272, 190);
			this.m_pnlMachining.TabIndex = 4;
			this.m_pnlMachining.Text = "加工控制";
			//
			// m_chkThreePointCalibration
			//
			this.m_chkThreePointCalibration.Location = new System.Drawing.Point(172, 5);
			this.m_chkThreePointCalibration.Name = "m_chkThreePointCalibration";
			this.m_chkThreePointCalibration.Size = new System.Drawing.Size(90, 22);
			this.m_chkThreePointCalibration.TabIndex = 0;
			this.m_chkThreePointCalibration.Text = "三點校正";
			//
			// m_lblFeedOverrideTitle
			//
			this.m_lblFeedOverrideTitle.Location = new System.Drawing.Point(10, 36);
			this.m_lblFeedOverrideTitle.Name = "m_lblFeedOverrideTitle";
			this.m_lblFeedOverrideTitle.Size = new System.Drawing.Size(110, 22);
			this.m_lblFeedOverrideTitle.TabIndex = 1;
			this.m_lblFeedOverrideTitle.Text = "進給倍率";
			this.m_lblFeedOverrideTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// m_lblFeedOverrideValue
			//
			this.m_lblFeedOverrideValue.Location = new System.Drawing.Point(168, 36);
			this.m_lblFeedOverrideValue.Name = "m_lblFeedOverrideValue";
			this.m_lblFeedOverrideValue.Role = CommonUI.Theme.TextRole.Value;
			this.m_lblFeedOverrideValue.Size = new System.Drawing.Size(94, 22);
			this.m_lblFeedOverrideValue.TabIndex = 2;
			this.m_lblFeedOverrideValue.Text = "[100]%";
			this.m_lblFeedOverrideValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// m_trbFeedOverride
			//
			this.m_trbFeedOverride.AutoSize = false;
			this.m_trbFeedOverride.LargeChange = 10;
			this.m_trbFeedOverride.Location = new System.Drawing.Point(10, 64);
			this.m_trbFeedOverride.Maximum = 200;
			this.m_trbFeedOverride.Name = "m_trbFeedOverride";
			this.m_trbFeedOverride.Size = new System.Drawing.Size(252, 28);
			this.m_trbFeedOverride.TabIndex = 3;
			this.m_trbFeedOverride.TickStyle = System.Windows.Forms.TickStyle.None;
			this.m_trbFeedOverride.Value = 100;
			//
			// m_btnStart
			//
			this.m_btnStart.Location = new System.Drawing.Point(10, 98);
			this.m_btnStart.Name = "m_btnStart";
			this.m_btnStart.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnStart.Size = new System.Drawing.Size(123, 38);
			this.m_btnStart.TabIndex = 4;
			this.m_btnStart.Text = "開始";
			//
			// m_btnPause
			//
			this.m_btnPause.Location = new System.Drawing.Point(139, 98);
			this.m_btnPause.Name = "m_btnPause";
			this.m_btnPause.Size = new System.Drawing.Size(123, 38);
			this.m_btnPause.TabIndex = 5;
			this.m_btnPause.Text = "暫停";
			//
			// m_btnContinue
			//
			this.m_btnContinue.Location = new System.Drawing.Point(10, 142);
			this.m_btnContinue.Name = "m_btnContinue";
			this.m_btnContinue.Size = new System.Drawing.Size(123, 38);
			this.m_btnContinue.TabIndex = 6;
			this.m_btnContinue.Text = "繼續";
			//
			// m_btnStop
			//
			this.m_btnStop.Location = new System.Drawing.Point(139, 142);
			this.m_btnStop.Name = "m_btnStop";
			this.m_btnStop.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnStop.Size = new System.Drawing.Size(123, 38);
			this.m_btnStop.TabIndex = 7;
			this.m_btnStop.Text = "停止";
			//
			// CNCPanel
			//
			this.AutoScroll = false;
			this.Controls.Add( this.m_pnlCommands );
			this.Name = "CNCPanel";
			this.Padding = new System.Windows.Forms.Padding( 1, 4, 1, 4 );
			this.Size = new System.Drawing.Size(288, 756);
			this.m_pnlCommands.ResumeLayout( false );
			this.m_pnlAuxiliary.ResumeLayout( false );
			this.m_pnlJog.ResumeLayout( false );
			this.m_pnlHome.ResumeLayout( false );
			this.m_pnlStrategy.ResumeLayout( false );
			this.m_pnlMachining.ResumeLayout( false );
			( (System.ComponentModel.ISupportInitialize)( this.m_trbFeedOverride ) ).EndInit();
			this.ResumeLayout( false );
		}

		#endregion

		private System.Windows.Forms.Panel m_pnlCommands;
		private CommonUI.Controls.SectionPanel m_pnlAuxiliary;
		private CommonUI.Controls.ThemedButton m_btnOuterEquipParameters;
		private CommonUI.Controls.ThemedButton m_btnShutter;
		private CommonUI.Controls.ThemedButton m_btnAir;
		private CommonUI.Controls.ThemedButton m_btnFollow;
		private CommonUI.Controls.ThemedButton m_btnLaser;
		private CommonUI.Controls.ThemedButton m_btnFocus;
		private CommonUI.Controls.ThemedButton m_btnCalibration;
		private CommonUI.Controls.SectionPanel m_pnlJog;
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
		private CommonUI.Controls.SectionPanel m_pnlHome;
		private CommonUI.Controls.ThemedButton m_btnHomeProgram;
		private CommonUI.Controls.ThemedButton m_btnHomeMachine;
		private CommonUI.Controls.ThemedButton m_btnSafePosition;
		private CommonUI.Controls.SectionPanel m_pnlStrategy;
		private CommonUI.Controls.ThemedButton m_btnResumeCutParameters;
		private CommonUI.Controls.ThemedButton m_btnDryRun;
		private CommonUI.Controls.ThemedButton m_btnResumeCutting;
		private CommonUI.Controls.ThemedButton m_btnHandwheelSimulation;
		private CommonUI.Controls.SectionPanel m_pnlMachining;
		private System.Windows.Forms.CheckBox m_chkThreePointCalibration;
		private CommonUI.Controls.ThemedLabel m_lblFeedOverrideTitle;
		private CommonUI.Controls.ThemedLabel m_lblFeedOverrideValue;
		private System.Windows.Forms.TrackBar m_trbFeedOverride;
		private CommonUI.Controls.ThemedButton m_btnStart;
		private CommonUI.Controls.ThemedButton m_btnPause;
		private CommonUI.Controls.ThemedButton m_btnContinue;
		private CommonUI.Controls.ThemedButton m_btnStop;
	}
}
