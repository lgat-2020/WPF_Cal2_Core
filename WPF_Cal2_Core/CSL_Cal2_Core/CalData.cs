using System;
using System.Collections.Generic;
using System.Text;

namespace CSL_Cal2_Core
{
    #region 週間日データ

    /// <summary>
    /// 週間日データ
    /// </summary>
    public class DayWeekData
    {
        // 週間表示日
        public string WD0 { get; private set; }
        public string WD1 { get; private set; }
        public string WD2 { get; private set; }
        public string WD3 { get; private set; }
        public string WD4 { get; private set; }
        public string WD5 { get; private set; }
        public string WD6 { get; private set; }

        public DayWeekData() : this( new string[] { " ", " ", " ", " ", " ", " ", " " } )
        {
        }

        public DayWeekData( string[] Days )
        {
            this.WD0 = Days[ 0 ];
            this.WD1 = Days[ 1 ];
            this.WD2 = Days[ 2 ];
            this.WD3 = Days[ 3 ];
            this.WD4 = Days[ 4 ];
            this.WD5 = Days[ 5 ];
            this.WD6 = Days[ 6 ];
        }

        public void SetDays( string[] str )
        {
            this.WD0 = str[ 0 ];
            this.WD1 = str[ 1 ];
            this.WD2 = str[ 2 ];
            this.WD3 = str[ 3 ];
            this.WD4 = str[ 4 ];
            this.WD5 = str[ 5 ];
            this.WD6 = str[ 6 ];
        }

        public string[] GetDays()
        {
            string[] ret = new string[ 7 ];
            ret[ 0 ] = this.WD0;
            ret[ 1 ] = this.WD1;
            ret[ 2 ] = this.WD2;
            ret[ 3 ] = this.WD3;
            ret[ 4 ] = this.WD4;
            ret[ 5 ] = this.WD5;
            ret[ 6 ] = this.WD6;

            return ret;
        }
    }

    #endregion 週間日データ

    #region 日情報構造体

    /// <summary>
    /// 日種別
    /// </summary>
    public struct DayType
    {
        /// <summary>
        /// 祝日
        /// </summary>
        public const string Holiday = "h";

        /// <summary>
        /// 振替休日
        /// </summary>
        public const string SubHoliday = "s";

        /// <summary>
        /// 国民の休日
        /// </summary>
        public const string ExHoliday = "e";

        /// <summary>
        /// 休暇
        /// </summary>
        public const string Offday = "o";

        /// <summary>
        /// 通常日
        /// </summary>
        public const string General = "g";

        /// <summary>
        /// 日曜日
        /// </summary>
        public const string Sunday = "u";

        /// <summary>
        /// 土曜日
        /// </summary>
        public const string Saturday = "a";

        /// <summary>
        /// 先月日
        /// </summary>
        public const string PrevMon = "p";

        /// <summary>
        /// 翌月日
        /// </summary>
        public const string NextMon = "n";
    }

    #endregion 日情報構造体

    #region カレンダー構成

    // カレンダー構成データ
    public class CalData
    {
        #region フィールド

        /// <summary>
        /// 月初月末追加情報フラグ
        /// </summary>
        private bool AddInfo;

        #endregion フィールド

        #region プロパティ

        /// <summary>
        /// 基準年
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 基準月
        /// </summary>
        public int Mon { get; set; }

        /// <summary>
        /// 基準日
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 当月の日数
        /// </summary>
        public int MonDays { get; set; }

        /// <summary>
        /// 当月の最初の曜日
        /// </summary>
        public int MonFirstWDay { get; set; }

        /// <summary>
        /// 当月の週数
        /// </summary>
        public int MonWeeks { get; set; }

        /// <summary>
        /// 基準日の含まれる週
        /// </summary>
        public int Week
        {
            get
            {
                return ( Day + Convert.ToInt32( new DateTime( Year, Mon, 1 ).DayOfWeek ) - 1 ) / 7 + 1;
            }
        }

        /// <summary>
        /// 週ごとの日データ
        /// </summary>
        public DayWeekData[] DayData { get; set; }

        /// <summary>
        /// 週ごとの日種別情報
        /// </summary>
        public DayWeekData[] DayTypeInfo { get; set; }

        #endregion プロパティ

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dt"></param>
        public CalData( DateTime dt, bool addInfo = false ) : this( dt.Year, dt.Month, dt.Day, addInfo )
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        public CalData( int year, int mon, int day, bool addInfo = false )
        {
            // 月初、月末追加情報
            AddInfo = addInfo;
            // 基本情報初期化
            Init( year, mon, day );
            // 日情報作成
            CreateCalData();
        }

        #endregion コンストラクタ

        // 公開メソッド

        #region 指定日の週を取得する

        /// <summary>
        /// 指定日の週を取得する
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static int GetWeek( int year, int mon, int day )
        {
            return ( day + Convert.ToInt32( new DateTime( year, mon, 1 ).DayOfWeek ) - 1 ) / 7 + 1;
        }

        #endregion 指定日の週を取得する

        // 内部メソッド

        #region 初期化

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        private void Init( int year, int mon, int day )
        {
            this.Year = year;
            this.Mon = mon;
            this.Day = day;

            //int monDays = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 }[ Mon - 1 ];
            //if ( Mon == 2 ) monDays += ( DateTime.IsLeapYear( Year ) ) ? 1 : 0;
            //this.MonDays = monDays;
            this.MonDays = DateTime.DaysInMonth( Year, Mon );

            DateTime dt = new DateTime( Year, Mon, 1 );
            this.MonFirstWDay = Convert.ToInt32( dt.DayOfWeek );

            this.MonWeeks = GetWeek( Year, Mon, MonDays );

            DayData = new DayWeekData[ MonWeeks ];
            DayTypeInfo = new DayWeekData[ MonWeeks ];
        }

        #endregion 初期化

        #region カレンダーデータ作成

        /// <summary>
        /// カレンダーデータ作成
        /// </summary>
        private void CreateCalData()
        {
            // 今月のカレンダーデータ作成
            CreateCalInfoMon();

            // 追加情報フラグ確認
            if ( AddInfo == true )
            {
                // 月初以前と月末以降に先月/翌月の情報を付加する

                int wday;
                int day;
                string[] FirstDays = this.DayData[ 0 ].GetDays();
                string[] LastDays = this.DayData[ this.MonWeeks - 1 ].GetDays();

                // 先月データ作成
                int pYear = this.Year;
                int pMon = this.Mon - 1;
                if ( pMon == 0 )
                {
                    pMon = 12;
                    pYear--;
                }
                CalData PrevCal = new CalData( pYear, pMon, 1 );

                // 月初以前

                // 先月末日の曜日
                int pWDay = Convert.ToInt32( new DateTime( PrevCal.Year, PrevCal.Mon, PrevCal.MonDays ).DayOfWeek );
                if ( pWDay < 6 )
                {
                    // 今月初日以前の空白を先月最終週の日で埋める
                    day = PrevCal.MonDays - pWDay;
                    for ( wday = 0; wday <= pWDay; wday++ )
                    {
                        FirstDays[ wday ] = ( day + wday ).ToString();
                    }
                }

                // 月末以降

                // 今月末日の曜日
                int nWDay = Convert.ToInt32( new DateTime( Year, Mon, MonDays ).DayOfWeek );
                day = 1;
                // 今月末日以降の空白を1から連番で埋める
                for ( wday = nWDay + 1; wday < 7; wday++, day++ )
                {
                    LastDays[ wday ] = day.ToString();
                }

                // 初週、最終週更新
                this.DayData[ 0 ].SetDays( FirstDays );
                this.DayData[ MonWeeks - 1 ].SetDays( LastDays );
            }
        }

        #endregion カレンダーデータ作成

        #region 指定月カレンダー情報を作成する

        /// <summary>
        /// 指定月カレンダー情報を作成する
        /// </summary>
        /// <param name="y"></param>
        /// <param name="m"></param>
        private void CreateCalInfoMon()
        {
            // 休暇情報読み込み
            List<OffDayInfo> OffdayList = CalOffDay.ReadOffdayFile();

            // 初週
            int wday = 0;
            int day = 1;
            string[] Days = new string[ 7 ];
            string[] DaysColor = new string[ 7 ];

            for ( wday = 0; wday < this.MonFirstWDay; wday++ )
            {
                Days[ wday ] = "";
                DaysColor[ wday ] = DayType.PrevMon;
            }
            for ( wday = this.MonFirstWDay; wday < 7; wday++, day++ )
            {
                Days[ wday ] = day.ToString();
                DaysColor[ wday ] = DayColorInfo( this.Year, this.Mon, day, wday, OffdayList );
            }

            this.DayData[ 0 ] = new DayWeekData();
            this.DayTypeInfo[ 0 ] = new DayWeekData();

            this.DayData[ 0 ].SetDays( Days );
            this.DayTypeInfo[ 0 ].SetDays( DaysColor );

            // 2週目以降
            for ( int week = 1; week < this.MonWeeks - 1; week++ )
            {
                Days = new string[ 7 ];
                DaysColor = new string[ 7 ];

                for ( wday = 0; wday < 7; wday++ )
                {
                    Days[ wday ] = ( day + wday ).ToString();
                    DaysColor[ wday ] = DayColorInfo( this.Year, this.Mon, day + wday, wday, OffdayList );
                }

                this.DayData[ week ] = new DayWeekData();
                this.DayTypeInfo[ week ] = new DayWeekData();

                this.DayData[ week ].SetDays( Days );
                this.DayTypeInfo[ week ].SetDays( DaysColor );

                day += wday;
            }

            // 最終周
            Days = new string[ 7 ];
            DaysColor = new string[ 7 ];

            for ( wday = 0; day <= this.MonDays; day++, wday++ )
            {
                Days[ wday ] = day.ToString();
                DaysColor[ wday ] = DayColorInfo( this.Year, this.Mon, day, wday, OffdayList );
            }
            for ( ; wday < 7; wday++ )
            {
                Days[ wday ] = "";
                DaysColor[ wday ] = DayType.NextMon;
            }

            this.DayData[ this.MonWeeks - 1 ] = new DayWeekData();
            this.DayTypeInfo[ this.MonWeeks - 1 ] = new DayWeekData();

            this.DayData[ this.MonWeeks - 1 ].SetDays( Days );
            this.DayTypeInfo[ this.MonWeeks - 1 ].SetDays( DaysColor );

            // 終了
            return;

            // ローカル関数

            // 休暇種別ごとの識別色設定
            string DayColorInfo( int _year, int _mon, int _day, int _wday, List<OffDayInfo> lst )
            {
                OffDayInfo offday = new OffDayInfo( _year, _mon, _day, enmOffDayType.None, "", "" );

                if ( true == Holiday.isExistOffday( lst, offday, enmOffDayType.Holiday ) )
                {
                    // 祝日
                    return DayType.Holiday;
                }
                else if ( true == Holiday.isExistOffday( lst, offday, enmOffDayType.SubHoliday ) )
                {
                    // 振替休日
                    return DayType.SubHoliday;
                }
                else if ( true == Holiday.isExistOffday( lst, offday, enmOffDayType.ExHoliday ) )
                {
                    // 国民の休日
                    return DayType.ExHoliday;
                }
                else if ( true == Holiday.isExistOffday( lst, offday, enmOffDayType.OffWork ) )
                {
                    // 休暇
                    return DayType.Offday;
                }
                else
                {
                    if ( _wday == 0 )
                    {
                        // 日曜日
                        return DayType.Sunday;
                    }
                    else if ( _wday == 6 )
                    {
                        // 土曜日
                        return DayType.Saturday;
                    }
                    else
                    {
                        // 通常日
                        return DayType.General;
                    }
                }
            }
        }

        #endregion 指定月カレンダー情報を作成する

    }

    #endregion カレンダー構成
}
