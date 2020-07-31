using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_Cal2_Core
{
    /// <summary>
    /// AddOffdayWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AddOffdayWindow : Window
    {
        private AddOffday addOffday;
        private int cmbSelectedIndex;

        public AddOffdayWindow()
        {
            InitializeComponent();
        }

        // 内部メソッド

        #region 初期化処理

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Init()
        {
            // 種別リスト作成
            cmb_OffdayType.ItemsSource = addOffday.CreateOffdayTypes();

            // 初期化
            addOffday.Init();

            // 対象日の初期休暇情報設定
            addOffday.SetOffdayInfo();

            // 描画更新
            ViewUpdate();

            // 祝日選択時、休暇種別と休暇名称を変更不可にする
            if ( cmb_OffdayType.SelectedIndex == ( int )AddOffday.OffdayTypeIndex.Holiday )
            {
                cmb_OffdayType.IsEnabled = false;
                tb_Name.IsReadOnly = true;
            }

            cmb_OffdayType.SelectionChanged += cmb_OffdayType_SelectionChanged;
        }

        #endregion 初期化処理

        #region 描画内容更新

        /// <summary>
        /// 描画内容更新
        /// </summary>
        private void ViewUpdate()
        {
            // addOffday上のView用プロパティを読み込む

            tb_Date.DataContext = addOffday.offdayDate;
            tb_Name.DataContext = addOffday.offdayName;
            tb_Biko.DataContext = addOffday.offdayBiko;

            cmb_OffdayType.SelectedIndex = addOffday.offdatType.offdayType.Value;
            cmbSelectedIndex = cmb_OffdayType.SelectedIndex;
        }

        #endregion 描画内容更新

        #region 画面を閉じる

        /// <summary>
        /// 画面を閉じる
        /// </summary>
        private void WindowClose()
        {
            this.Close();
        }

        #endregion 画面を閉じる

        // 公開メソッド

        #region 画面開始

        /// <summary>
        /// 画面開始
        /// </summary>
        /// <param name="calData"></param>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        public void WindowStart( CSL_Cal2_Core.CalData calData, int year, int mon, int day )
        {
            addOffday = new AddOffday( calData, year, mon, day );

            // 初期化処理
            Init();

            // 画面表示
            this.ShowDialog();

            // 最前面表示
            this.Topmost = true;
        }

        #endregion 画面開始


        // イベント

        #region 休暇種別選択時イベント

        /// <summary>
        /// 休暇種別選択時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmb_OffdayType_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            int index = ( sender as ComboBox ).SelectedIndex;
            if (addOffday.OffdayTypeChanged( index ) == false)
            {
                // 無効時(祝日選択時)、選択を戻す
                ( ( ComboBox )sender ).SelectedIndex = cmbSelectedIndex;
            }

            ViewUpdate();
        }

        #endregion 休暇種別選択時イベント

        #region 休暇名称変更時イベント

        /// <summary>
        /// 休暇名称変更時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_Name_TextChanged( object sender, TextChangedEventArgs e )
        {
            // 双方向のReactivePropertyなら、このようなイベントは不要になる？

            string input = ( sender as TextBox ).Text;
            addOffday.offdayName.NameText.Value = input;
        }

        #endregion 休暇名称変更時イベント

        #region 備考変更時イベント

        /// <summary>
        /// 備考変更時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_Biko_TextChanged( object sender, TextChangedEventArgs e )
        {
            string input = ( sender as TextBox ).Text;
            addOffday.offdayBiko.BikoText.Value = input;
        }

        #endregion 備考変更時イベント

        #region 更新ボタン押下イベント

        /// <summary>
        /// 更新ボタン押下イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Update_Click( object sender, RoutedEventArgs e )
        {
            // 更新処理
            addOffday.OffdayInfoUpdate();
        }

        #endregion 更新ボタン押下イベント

        #region 終了ボタン押下イベント

        /// <summary>
        /// 終了ボタン押下イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Exit_Click( object sender, RoutedEventArgs e )
        {
            WindowClose();
        }

        #endregion 終了ボタン押下イベント

    }
}
