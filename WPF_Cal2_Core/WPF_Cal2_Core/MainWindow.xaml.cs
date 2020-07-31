using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_Cal2_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WPF_Cal cal;
        private int ViewYear;
        private int ViewMon;
        private bool ViewUpdateComplete;

        private bool EventGuard = false;

        private static readonly double DefaultWindowHeight = 350.0;
        private static readonly double DefaultOffdayNameHeight = 20.0;
        private static readonly double DefaultBikoHeight = 50.0;

        public int BaseYear { get; private set; }
        public int BaseMon { get; private set; }
        public int BaseDay { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            DateTime dt = DateTime.Now;

            BaseYear = dt.Year;
            BaseMon = dt.Month;
            BaseDay = dt.Day;

            ViewYear = BaseYear;
            ViewMon = BaseMon;
        }

        /// <summary>
        /// カレンダー情報作成
        /// </summary>
        private void CreateCal()
        {
            cal = new WPF_Cal( ViewYear, ViewMon, BaseDay );

            // カレンダー表示用データ作成
            cal.CreateViewData();

            // 年月表示割り当て
            tb_DateText.DataContext = cal.DateText;
        }

        /// <summary>
        /// 描画更新
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        //private async Task CalenderView()
        private void CalenderView()
        {
            dg_Cal.LayoutUpdated += dg_Cal_LayoutUpdated;

            ViewUpdateComplete = false;

            // 曜日表示割り当て
            dg_Youbi.ItemsSource = cal.Youbi.YoubiLineInfo;

            // カレンダーデータ割り当て
            dg_Cal.ItemsSource = cal.Days.WeekDays;

            // カレンダーサイズ(表示行数による縦サイズ)再設定
            dg_Cal.Height = dg_Cal.Items.Count * dg_Cal.RowHeight + 1;

            // 休暇名称・備考欄非表示
            //SetNameBikoVisible( false );
            SetNameBikoVisible();

            // UI描画更新
            dg_Cal.UpdateLayout();

            //// 描画更新待機
            //while (!ViewUpdateComplete )
            //{
            //    await Task.Delay( 25 );
            //}
        }

        /// <summary>
        /// カレンダーデータグリッド初期化
        /// </summary>
        private void CalInit()
        {
            dg_Cal.ItemsSource = new[]
            {
                new CSL_Cal2_Core.DayWeekData(),
                new CSL_Cal2_Core.DayWeekData(),
                new CSL_Cal2_Core.DayWeekData(),
                new CSL_Cal2_Core.DayWeekData(),
                new CSL_Cal2_Core.DayWeekData(),
                new CSL_Cal2_Core.DayWeekData(),
            };
        }

        /// <summary>
        /// カレンダーセル着色
        /// </summary>
        private void SetColor()
        {
            int day = 0;
            bool ThisMonFlag = false;
            for ( int week = 0; week < cal.DayTypeInfo.Length; week++ )
            {
                string[] weekDayType = cal.DayTypeInfo[ week ].GetDays();

                for ( int wday = 0; wday < 7; wday++ )
                {
                    // 日情報
                    string dayType = weekDayType[ wday ];

                    // 対応するセル取得
                    DataGridCell cell = GetDataGridCell( dg_Cal, week, wday );

                    // 取得できない場合、処理しない
                    if ( cell == null )
                    {
                        continue;
                    }

                    ThisMonFlag = true;

                    // デフォルト色
                    cell.Background = CellBackColor.DefaultBackColor;
                    cell.Foreground = CellForeColor.DefaultForeColor;

                    // 日情報別にセルに着色する
                    switch ( dayType )
                    {
                        case CSL_Cal2_Core.DayType.PrevMon:
                        case CSL_Cal2_Core.DayType.NextMon:
                            cell.Foreground = CellForeColor.NotThisMonthForeColor;
                            ThisMonFlag = false;
                            break;

                        case CSL_Cal2_Core.DayType.Sunday:
                            cell.Foreground = CellForeColor.SundayForeColor;
                            day++;
                            break;

                        case CSL_Cal2_Core.DayType.Saturday:
                            cell.Foreground = CellForeColor.SaturdayForeColor;
                            day++;
                            break;

                        case CSL_Cal2_Core.DayType.Holiday:
                            cell.Background = CellBackColor.HolidayBackColor;
                            cell.Foreground = CellForeColor.HolidayForeColor;
                            day++;
                            break;

                        case CSL_Cal2_Core.DayType.SubHoliday:
                            cell.Background = CellBackColor.SubHolidayBackColor;
                            cell.Foreground = CellForeColor.SubHolidayForeColor;
                            day++;
                            break;

                        case CSL_Cal2_Core.DayType.ExHoliday:
                            cell.Background = CellBackColor.SubHolidayBackColor;
                            cell.Foreground = CellForeColor.SubHolidayForeColor;
                            day++;
                            break;

                        case CSL_Cal2_Core.DayType.Offday:
                            cell.Background = CellBackColor.OffdayBackColor;
                            cell.Foreground = CellForeColor.OffdayForeColor;
                            day++;
                            break;

                        case CSL_Cal2_Core.DayType.General:
                            day++;
                            break;
                    }

                    // 今日の場合
                    if ( day == BaseDay
                        && cal.Mon == BaseMon
                        && cal.Year == BaseYear
                        && ThisMonFlag == true )
                    {
                        cell.Foreground = CellForeColor.TodayForeColor;
                        cell.FontWeight = FontWeights.Bold;
                    }
                }
            }
        }

        /// <summary>
        /// セルを取得
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        private DataGridCell GetDataGridCell( DataGrid dataGrid, int rowIndex, int columnIndex )
        {
            if ( dataGrid.Items == null )
            {
                return null;
            }

            // 行データ取得
            DataGridRow dgRow = dataGrid.ItemContainerGenerator.ContainerFromIndex( rowIndex ) as DataGridRow;
            if ( dgRow == null )
            {
                return null;
            }

            // セルを取得
            DataGridCell dgCell = dataGrid.Columns[ columnIndex ].GetCellContent( dgRow ).Parent as DataGridCell;

            return dgCell;
        }

        /// <summary>
        /// 休暇名称・備考欄表示設定
        /// </summary>
        private void SetNameBikoVisible()
        {
            double NameHeight = 1.0;
            double BikoHeight = 1.0;
            double MainWindowHeight = DefaultWindowHeight - ( DefaultOffdayNameHeight + DefaultBikoHeight );

            tb_Name.Visibility = Visibility.Hidden;
            tb_Biko.Visibility = Visibility.Hidden;

            if ( !string.IsNullOrEmpty( cal.NameBiko.Name.Value ) )
            {
                NameHeight = DefaultOffdayNameHeight;
                tb_Name.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(cal.NameBiko.Biko.Value))
            {
                BikoHeight = DefaultBikoHeight;
                tb_Biko.Visibility = Visibility.Visible;
            }

            grd_OffdayName.Height = new GridLength( NameHeight, GridUnitType.Pixel );
            grd_Biko.Height = new GridLength( BikoHeight, GridUnitType.Pixel );

            CalWindow.Height = MainWindowHeight + NameHeight + BikoHeight;
        }

        /// <summary>
        /// 休暇名称・備考の表示設定を切り替える
        /// </summary>
        /// <param name="Visible"></param>
        private void SetNameBikoVisible( bool Visible )
        {
            if ( Visible )
            {
                tb_Name.Visibility = Visibility.Visible;
                tb_Biko.Visibility = Visibility.Visible;

                grd_OffdayName.Height = new GridLength( DefaultOffdayNameHeight, GridUnitType.Pixel );
                grd_Biko.Height = new GridLength( DefaultBikoHeight, GridUnitType.Pixel );
                CalWindow.Height = DefaultWindowHeight;
            }
            else
            {
                tb_Name.Visibility = Visibility.Hidden;
                tb_Biko.Visibility = Visibility.Hidden;

                grd_OffdayName.Height = new GridLength( 1.0, GridUnitType.Pixel );
                grd_Biko.Height = new GridLength( 1.0, GridUnitType.Pixel );
                CalWindow.Height = DefaultWindowHeight - ( DefaultOffdayNameHeight + DefaultBikoHeight );
            }

        }

        // イベント

        /// <summary>
        /// 画面ロード時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            EventGuard = true;

            // カレンダー用グリッド初期化
            // 空白の配列を割り当てることで、データグリッド上にセルを作成する
            CalInit();

            // 初回描画
            CreateCal();
            CalenderView();

            EventGuard = false;
        }

        /// <summary>
        /// カレンダー描画更新完了時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg_Cal_LayoutUpdated( object sender, EventArgs e )
        {
            dg_Cal.LayoutUpdated -= dg_Cal_LayoutUpdated;
            ViewUpdateComplete = true;

            // 休暇情報によりセルの色変更
            SetColor();
        }

        /// <summary>
        /// 先月ボタンクリック時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void left_Button_Click( object sender, RoutedEventArgs e )
        {
            // 先月のカレンダーに切り替え
            ViewMon--;
            if ( ViewMon <= 0 )
            {
                ViewMon = 12;
                ViewYear--;
            }

            EventGuard = true;

            // 再描画
            CreateCal();
            CalenderView();

            EventGuard = false;
        }

        /// <summary>
        /// 来月ボタンクリック時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void right_Button_Click( object sender, RoutedEventArgs e )
        {
            // 来月のカレンダーに切り替え
            ViewMon++;
            if ( ViewMon > 12 )
            {
                ViewMon = 1;
                ViewYear++;
            }

            EventGuard = true;

            // 再描画
            CreateCal();
            CalenderView();

            EventGuard = false;
        }

        /// <summary>
        /// セル選択時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg_Cal_GotFocus( object sender, RoutedEventArgs e )
        {
            if ( e.OriginalSource.GetType() == typeof( DataGridCell ) )
            {
                DataGridCell cell = e.OriginalSource as DataGridCell;

                cal.CellClick( cell );
                SetNameBikoVisible();
            }

            tb_Name.DataContext = cal.NameBiko;
            tb_Biko.DataContext = cal.NameBiko;
        }

        /// <summary>
        /// セルダブルクリック時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg_Cal_MouseDoubleClick( object sender, MouseButtonEventArgs e )
        {
            var elem = e.MouseDevice.DirectlyOver as FrameworkElement;
            if ( null != elem )
            {
                DataGridCell cell = elem.Parent as DataGridCell;
                if ( null == cell )
                {
                    cell = elem.TemplatedParent as DataGridCell;
                }
                if ( null != cell )
                {
                    return;
                }

                // セル位置を取得
                int row = DataGridRow.GetRowContainingElement( cell ).GetIndex();
                int col = cell.Column.DisplayIndex;

                // ダブルクリック時処理
                if ( cal.CellDoubleClick( cell ) )
                {
                    EventGuard = true;

                    // 再描画
                    CreateCal();
                    CalenderView();

                    // データグリッドをフォーカス
                    dg_Cal.Focus();

                    // データグリッドのカレントセルを指定する
                    DataGridCellInfo cellInfo = new DataGridCellInfo( dg_Cal.Items[ row ], dg_Cal.Columns[ col ] );
                    dg_Cal.CurrentCell = cellInfo;

                    EventGuard = false;
                }
            }
        }

        /// <summary>
        /// 年月手動変更時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_DateText_TextChanged( object sender, TextChangedEventArgs e )
        {
            if ( EventGuard )
            {
                return;
            }
            bool EventGuard_tmp = EventGuard;
            EventGuard = true;

            tb_DateText.TextChanged -= tb_DateText_TextChanged;

            string text = ( sender as TextBox ).Text;
            // 手動変更時処理
            if ( cal.DateTextChanged( text ) )
            {
                string[] words = text.Split( '/' );
                int year = 0;
                int mon = 0;
                Int32.TryParse( words[ 0 ], out year );
                Int32.TryParse( words[ 1 ], out mon );
                ViewYear = year;
                ViewMon = mon;

                // 再描画
                CreateCal();
                CalenderView();
            }

            EventGuard = EventGuard_tmp;
            tb_DateText.TextChanged += tb_DateText_TextChanged;
        }
    }
}
