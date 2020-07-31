using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text;
using Microsoft.VisualBasic;
using Reactive.Bindings;
using System.Windows.Media;
using System.Windows.Controls;
using System.Linq;
using System.Text.RegularExpressions;

namespace WPF_Cal2_Core
{
    #region セル背景色

    /// <summary>
    /// セル背景色
    /// </summary>
    static class CellBackColor
    {
        /// <summary>
        /// 通常背景色
        /// </summary>
        public static readonly SolidColorBrush DefaultBackColor = null;

        /// <summary>
        /// 祝日背景色
        /// </summary>
        public static readonly SolidColorBrush HolidayBackColor = Brushes.OrangeRed;

        /// <summary>
        /// 振替休日背景色
        /// </summary>
        public static readonly SolidColorBrush SubHolidayBackColor = Brushes.Red;

        /// <summary>
        /// 国民の休日背景色
        /// </summary>
        public static readonly SolidColorBrush ExHolidayBackColore = Brushes.Red;

        /// <summary>
        /// 休暇背景色
        /// </summary>
        public static readonly SolidColorBrush OffdayBackColor = Brushes.Yellow;

        /// <summary>
        /// 今月以外の背景色
        /// </summary>
        public static readonly SolidColorBrush NotThisMonthBackColor = null;
    }

    #endregion セル背景色

    #region セル文字色

    /// <summary>
    /// セル文字色
    /// </summary>
    static class CellForeColor
    {
        /// <summary>
        /// 通常文字色
        /// </summary>
        public static readonly SolidColorBrush DefaultForeColor = Brushes.Black;

        /// <summary>
        /// 祝日文字色
        /// </summary>
        public static readonly SolidColorBrush HolidayForeColor = Brushes.White;

        /// <summary>
        /// 振替休日文字色
        /// </summary>
        public static readonly SolidColorBrush SubHolidayForeColor = Brushes.White;

        /// <summary>
        /// 国民の休日文字色
        /// </summary>
        public static readonly SolidColorBrush ExHolidayForeColor = Brushes.White;

        /// <summary>
        /// 休暇文字色
        /// </summary>
        public static readonly SolidColorBrush OffdayForeColor = Brushes.Black;

        /// <summary>
        /// 今月以外の文字色
        /// </summary>
        public static readonly SolidColorBrush NotThisMonthForeColor = Brushes.Gray;

        /// <summary>
        /// 今日の文字色
        /// </summary>
        public static readonly SolidColorBrush TodayForeColor = Brushes.Green;

        /// <summary>
        /// 日曜日の文字色
        /// </summary>
        public static readonly SolidColorBrush SundayForeColor = Brushes.Red;

        /// <summary>
        /// 土曜日の文字色
        /// </summary>
        public static readonly SolidColorBrush SaturdayForeColor = Brushes.Blue;
    }

    #endregion セル文字色

    // カレンダー描画用クラス
    public class WPF_Cal
    {
        #region フィールド

        private CSL_Cal2_Core.CalData calData;
        private List<CSL_Cal2_Core.OffDayInfo> OffdayList;

        #endregion フィールド

        #region プロパティ

        /// <summary>
        /// 基準年
        /// </summary>
        public int Year { get; private set; }

        /// <summary>
        /// 基準月
        /// </summary>
        public int Mon { get; private set; }

        /// <summary>
        /// 基準日
        /// </summary>
        public int Day { get; private set; }

        /// <summary>
        /// 週数
        /// </summary>
        public int MonWeeks { get; private set; }

        /// <summary>
        /// 年月表示
        /// </summary>
        public DateTextProperty DateText { get; private set; }

        /// <summary>
        /// 曜日表示
        /// </summary>
        public YoubiLineProperty Youbi { get; private set; }

        /// <summary>
        /// 日表示
        /// </summary>
        public DayLineProperty Days { get; private set; }

        /// <summary>
        /// 休暇情報
        /// </summary>
        public OffdayNameBikoProperty NameBiko { get; private set; }

        /// <summary>
        /// 週ごとの日種別情報
        /// </summary>
        public CSL_Cal2_Core.DayWeekData[] DayTypeInfo { get; set; }

        #endregion プロパティ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WPF_Cal() : this( DateTime.Now )
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dt"></param>
        public WPF_Cal( DateTime dt ) : this( dt.Year, dt.Month, dt.Day )
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dt"></param>
        public WPF_Cal( int year, int mon, int day )
        {
            Year = year;
            Mon = mon;
            Day = day;

            CSL_Cal2_Core.CalOffDay.CreateOffdayList( Year );
            calData = new CSL_Cal2_Core.CalData( year, mon, day, true );
            MonWeeks = calData.MonWeeks;

            DayTypeInfo = new CSL_Cal2_Core.DayWeekData[ MonWeeks ];
            for ( int i = 0; i < MonWeeks; i++ )
            {
                DayTypeInfo[ i ] = new CSL_Cal2_Core.DayWeekData( calData.DayTypeInfo[ i ].GetDays() );
            }

            DateText = new DateTextProperty();
            Youbi = new YoubiLineProperty();
            Days = new DayLineProperty( calData );
            NameBiko = new OffdayNameBikoProperty();

            OffdayList = new List<CSL_Cal2_Core.OffDayInfo>();
        }

        /// <summary>
        /// カレンダー描画
        /// </summary>
        public void CreateViewData()
        {
            // 年月表示
            DateText.DateText.Value = GetDateString();

            // 曜日表示
            SetYoubi();

            // 日表示
            SetDays();

            // 休暇リスト取得
            OffdayList = CSL_Cal2_Core.CalOffDay.ReadOffdayFile();
        }

        // 年月表示設定
        public string GetDateString()
        {
            return string.Format( "{0:0000}/{1:00}", Year, Mon );
        }

        // 曜日表示設定
        public void SetYoubi()
        {
            Youbi.YoubiLineInfo[ 0 ].SetWeek( new string[] { "日", "月", "火", "水", "木", "金", "土" } );
        }

        // 日表示
        public void SetDays()
        {
            for ( int i = 0; i < calData.MonWeeks; i++ )
            {
                Days.WeekDays[ i ].SetWeek( calData.DayData[ i ].GetDays() );
            }
        }

        // セルクリック時処理
        public bool CellClick( DataGridCell cell )
        {
            bool ret = false;

            // セルから値を取得
            string cellText = ( ( TextBlock )cell.Content ).Text;
            int day = 0;
            Int32.TryParse( cellText, out day );

            // 休暇情報取得
            List<CSL_Cal2_Core.OffDayInfo> lst = OffdayList
                .Where( x => x.Year == calData.Year && x.Mon == calData.Mon && x.Day == day && ( x.OffDayType != CSL_Cal2_Core.enmOffDayType.None || x.Biko != "" ) )
                .ToList();

            if ( lst.Count > 0 )
            {
                // 休暇日が存在する場合、休暇情報を更新する
                NameBiko.Name.Value = lst[ 0 ].OffDayName;
                NameBiko.Biko.Value = lst[ 0 ].Biko;
                ret = true;
            }
            else
            {
                NameBiko.Clear();
            }

            return ret;
        }

        // セルダブルクリック時処理
        public bool CellDoubleClick( DataGridCell cell )
        {
            string strDay = ( ( TextBlock )cell.Content ).Text;
            Brush cellForeColor = cell.Foreground;
            Brush cellBackColor = cell.Background;

            if ( cellForeColor == CellForeColor.NotThisMonthForeColor )
            {
                // 今月でなければ終了
                return false;
            }

            // クリック箇所の年月日
            int cYear = calData.Year;
            int cMon = calData.Mon;
            int cDay = 0;

            Int32.TryParse( strDay, out cDay );

            // 休暇設定画面立ち上げ
            AddOffdayWindow addWnd = new AddOffdayWindow();
            addWnd.WindowStart( calData, cYear, cMon, cDay );

            return true;
        }

        // 年月テキスト手動変更時
        public bool DateTextChanged( string text )
        {
            return Regex.IsMatch( text, @"\d{4}/\d{1,2}" );
        }
    }


    // 年月表示
    public class DateTextProperty
    {
        public ReactiveProperty<string> DateText { get; set; }

        public DateTextProperty()
        {
            DateText = new ReactiveProperty<string>();
        }
    }


    // 週間表示
    public class WeekLineProperty
    {
        public ReactiveProperty<string> WD0 { get; private set; }
        public ReactiveProperty<string> WD1 { get; private set; }
        public ReactiveProperty<string> WD2 { get; private set; }
        public ReactiveProperty<string> WD3 { get; private set; }
        public ReactiveProperty<string> WD4 { get; private set; }
        public ReactiveProperty<string> WD5 { get; private set; }
        public ReactiveProperty<string> WD6 { get; private set; }

        public WeekLineProperty()
        {
            WD0 = new ReactiveProperty<string>();
            WD1 = new ReactiveProperty<string>();
            WD2 = new ReactiveProperty<string>();
            WD3 = new ReactiveProperty<string>();
            WD4 = new ReactiveProperty<string>();
            WD5 = new ReactiveProperty<string>();
            WD6 = new ReactiveProperty<string>();
        }

        public void SetWeek( string[] str )
        {
            WD0.Value = str[ 0 ];
            WD1.Value = str[ 1 ];
            WD2.Value = str[ 2 ];
            WD3.Value = str[ 3 ];
            WD4.Value = str[ 4 ];
            WD5.Value = str[ 5 ];
            WD6.Value = str[ 6 ];
        }
    }

    // 曜日表示
    public class YoubiLineProperty
    {
        public WeekLineProperty[] YoubiLineInfo { get; set; }

        public YoubiLineProperty()
        {
            this.YoubiLineInfo = new WeekLineProperty[ 1 ];
            this.YoubiLineInfo[ 0 ] = new WeekLineProperty();
        }
    }

    // 日表示
    public class DayLineProperty
    {
        public WeekLineProperty[] WeekDays { get; set; }

        public DayLineProperty( CSL_Cal2_Core.CalData calData )
        {
            WeekDays = new WeekLineProperty[ calData.MonWeeks ];
            for ( int i = 0; i < calData.MonWeeks; i++ )
            {
                WeekDays[ i ] = new WeekLineProperty();
            }
        }
    }

    // 休暇情報
    public class OffdayNameBikoProperty
    {
        public ReactiveProperty<string> Name { get; set; }
        public ReactiveProperty<string> Biko { get; set; }

        public OffdayNameBikoProperty()
        {
            Name = new ReactiveProperty<string>();
            Biko = new ReactiveProperty<string>();
        }

        public void Clear()
        {
            Name.Value = "";
            Biko.Value = "";
        }
    }
}
