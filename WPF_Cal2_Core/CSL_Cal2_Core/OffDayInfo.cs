using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CSL_Cal2_Core
{
    #region 休暇種別

    /// <summary>
    /// 休暇種別
    /// </summary>
    public enum enmOffDayType
    {
        /// <summary>
        /// 休暇ではない
        /// </summary>
        None = 0,

        /// <summary>
        /// 祝日
        /// </summary>
        Holiday,

        /// <summary>
        /// 振替休日
        /// </summary>
        SubHoliday,

        /// <summary>
        /// 国民の休日
        /// </summary>
        ExHoliday,

        /// <summary>
        /// 個人的な休暇
        /// </summary>
        OffWork,
    }

    #endregion 休暇種別

    #region 休暇情報

    /// <summary>
    /// 休暇情報クラス
    /// </summary>
    public class OffDayInfo
    {
        // 内部変数
        enmOffDayType _offdayType;
        int _offdayTypeNum;

        // プロパティ

        /// <summary>
        /// 年
        /// </summary>
        public int Year { get; private set; }

        /// <summary>
        /// 月
        /// </summary>
        public int Mon { get; private set; }

        /// <summary>
        /// 日
        /// </summary>
        public int Day { get; private set; }

        /// <summary>
        /// 休暇種別
        /// </summary>
        public enmOffDayType OffDayType
        {
            get
            {
                return _offdayType;
            }

            private set
            {
                _offdayType = value;
                switch ( value )
                {
                    case enmOffDayType.Holiday:
                        _offdayTypeNum = 1;
                        break;

                    case enmOffDayType.SubHoliday:
                        _offdayTypeNum = 2;
                        break;

                    case enmOffDayType.ExHoliday:
                        _offdayTypeNum = 3;
                        break;

                    case enmOffDayType.OffWork:
                        _offdayTypeNum = 4;
                        break;

                    case enmOffDayType.None:
                    default:
                        _offdayTypeNum = 0;
                        break;
                }
            }
        }

        /// <summary>
        /// 休暇種別
        /// </summary>
        public int OffDayTypeNum
        {
            get
            {
                return _offdayTypeNum;
            }

            private set
            {
                _offdayTypeNum = value;
                switch ( value )
                {
                    case 1:
                        _offdayType = enmOffDayType.Holiday;
                        break;

                    case 2:
                        _offdayType = enmOffDayType.SubHoliday;
                        break;

                    case 3:
                        _offdayType = enmOffDayType.ExHoliday;
                        break;

                    case 4:
                        _offdayType = enmOffDayType.OffWork;
                        break;

                    case 0:
                    default:
                        _offdayType = enmOffDayType.None;
                        break;
                }
            }
        }

        /// <summary>
        /// 休暇名称
        /// </summary>
        public string OffDayName { get; private set; }

        /// <summary>
        /// 備考
        /// </summary>
        public string Biko { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        /// <param name="offdayType"></param>
        /// <param name="offdayName"></param>
        /// <param name="biko"></param>
        public OffDayInfo( int year, int mon, int day, enmOffDayType offdayType, string offdayName, string biko = "" )
        {
            this.Year = year;
            this.Mon = mon;
            this.Day = day;
            this.OffDayType = offdayType;
            this.OffDayName = offdayName;
            this.Biko = biko;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        /// <param name="offdayTypeNum"></param>
        /// <param name="offdayName"></param>
        /// <param name="biko"></param>
        public OffDayInfo( int year, int mon, int day, int offdayTypeNum, string offdayName, string biko = "" )
        {
            this.Year = year;
            this.Mon = mon;
            this.Day = day;
            this.OffDayTypeNum = offdayTypeNum;
            this.OffDayName = offdayName;
            this.Biko = biko;
        }
    }

    #endregion 休暇情報

    #region 休暇リストクラス

    /// <summary>
    /// 休暇リスト作成クラス
    /// </summary>
    public static class CalOffDay
    {
        // 内部変数

        static private int Year;
        static private bool isLeapYear;

        // 固定変数

        private const string DefaultFileName = "OffdayList.txt";

        // 公開メソッド

        #region 祝日リスト作成
        /// <summary>
        /// 祝日情報リストを作成し、取得する
        /// </summary>
        /// <returns></returns>
        public static List<OffDayInfo> CreateOffdayList( int year )
        {
            Year = year;
            isLeapYear = DateTime.IsLeapYear( Year );

            // 最終的な休暇リスト
            List<OffDayInfo> OffdayList = new List<OffDayInfo>();

            // 祝日リスト
            List<OffDayInfo> HolidayList;

            // 個人休暇リスト
            List<OffDayInfo> PrivateOffdayList;

            // 通常日リスト
            List<OffDayInfo> NormalDayList;

            // 年間の祝日設定
            HolidayList = Holiday.GetHolidayList( Year );

            // リストファイル読み出し
            List<OffDayInfo> OffdayReadFileList = ReadOffdayFile();

            // 祝日情報抽出
            List<OffDayInfo> FileHolidayList = ExtractHoliday( OffdayReadFileList );

            // 祝日差異判定 自動で出力するデフォルト祝日にない設定があれば反映する
            HolidayList = HolidayCompare( HolidayList, FileHolidayList );

            // 振替休日の設定
            Holiday.SetSubHoliday( HolidayList );

            // 国民の休日設定
            Holiday.SetExHoliday( HolidayList );

            // 祝日情報を休暇リストへ反映
            foreach ( OffDayInfo offday in HolidayList )
            {
                OffdayList.Add( offday );
            }

            // 個人の休暇情報抽出
            PrivateOffdayList = ExtractOffday( OffdayReadFileList );

            // 休暇情報を休暇リストへ反映
            foreach ( OffDayInfo offday in PrivateOffdayList )
            {
                OffdayList.Add( offday );
            }

            // 通常日の情報抽出
            NormalDayList = ExtractNormalDay( OffdayReadFileList );

            // 通常日情報を休暇リストへ反映
            foreach ( OffDayInfo offday in NormalDayList )
            {
                OffdayList.Add( offday );
            }

            // 日付順ソート
            OffDayListSort( OffdayList );

            // ファイルへ保存
            WriteOffdayFile( OffdayList );

            // 終了
            return OffdayList;
        }
        #endregion 祝日リスト作成

        #region 休暇リストソート
        /// <summary>
        /// 休暇リストソート
        /// </summary>
        /// <param name="lst"></param>
        public static void OffDayListSort( List<OffDayInfo> lst )
        {
            lst.Sort( Compare );

            return;

            // List.Sort()用カスタムソート
            int Compare( OffDayInfo x, OffDayInfo y )
            {
                if ( x.Year < y.Year )
                {
                    return -1;
                }
                else if ( x.Year > y.Year )
                {
                    return 1;
                }
                else
                {
                    if ( x.Mon < y.Mon )
                    {
                        return -1;
                    }
                    else if ( x.Mon > y.Mon )
                    {
                        return 1;
                    }
                    else
                    {
                        if ( x.Day < y.Day )
                        {
                            return -1;
                        }
                        else if ( x.Day > y.Day )
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }
        #endregion 休暇リストソート

        #region 休暇情報の追加

        /// <summary>
        /// 休暇情報ファイルに追加
        /// </summary>
        /// <param name="offday"></param>
        /// <returns></returns>
        public static bool AddOffday( OffDayInfo offday )
        {
            return AddOffday( offday.Year, offday.Mon, offday.Day, offday.OffDayType, offday.OffDayName, offday.Biko );
        }

        /// <summary>
        /// 休暇情報ファイルに追加
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        /// <param name="offday_type"></param>
        /// <param name="offday_name"></param>
        /// <param name="biko"></param>
        /// <returns></returns>
        public static bool AddOffday( int year, int mon, int day, enmOffDayType offday_type, string offday_name, string biko )
        {
            List<OffDayInfo> dummy = new List<OffDayInfo>();

            // ファイル読み出し
            dummy = ReadOffdayFile();

            return AddOffday( year, mon, day, offday_type, offday_name, biko, dummy );
        }

        /// <summary>
        /// 休暇情報リストに休暇情報を追加し、ファイルへ保存する
        /// </summary>
        /// <param name="offday"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static bool AddOffday( OffDayInfo offday, List<OffDayInfo> lst )
        {
            return AddOffday( offday.Year, offday.Mon, offday.Day, offday.OffDayType, offday.OffDayName, offday.Biko, lst );
        }

        /// <summary>
        /// 休暇情報リストに休暇情報を追加し、ファイルへ保存する
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        /// <param name="offday_type"></param>
        /// <param name="offday_name"></param>
        /// <param name="biko"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static bool AddOffday( int year, int mon, int day, enmOffDayType offday_type, string offday_name, string biko, List<OffDayInfo> lst )
        {
            bool ret = false;

            // リストへ追加
            lst.Add( new OffDayInfo( year, mon, day, offday_type, offday_name, biko ) );
            // 日付順ソート
            OffDayListSort( lst );

            // ファイルへ保存
            try
            {
                WriteOffdayFile( lst );
                ret = true;
            }
            catch ( Exception e )
            {
                // 例外
                Console.WriteLine( e.Message );
                ret = false;
            }

            return ret;
        }

        #endregion 休暇情報の追加

        #region 休暇情報の削除

        /// <summary>
        /// 休暇ファイルから年月日を指定して休暇情報を削除
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static bool RemoveOffday( int year, int mon, int day )
        {
            // ファイル読み出し
            List<OffDayInfo> dummy = new List<OffDayInfo>();
            dummy = ReadOffdayFile();

            return RemoveOffday( year, mon, day, dummy );
        }

        /// <summary>
        /// 休暇リストから年月日を指定して休暇情報を削除し、ファイルへ保存
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static bool RemoveOffday( int year, int mon, int day, List<OffDayInfo> lst )
        {
            bool ret = false;

            if ( Holiday.isExistOffday( lst, year, mon, day ) == false )
            {
                // 削除なし
                return true;
            }

            // リストから削除
            lst.RemoveAll( x => x.Year == year && x.Mon == mon && x.Day == day );

            // ファイルへ保存
            try
            {
                WriteOffdayFile( lst );
                ret = true;
            }
            catch ( Exception e )
            {
                // 例外
                Console.WriteLine( e.Message );
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// 休暇ファイルから休暇名称を指定して休暇情報を削除
        /// </summary>
        /// <param name="offday_name"></param>
        /// <returns></returns>
        public static bool RemoveOffday( string offday_name )
        {
            // ファイル読み出し
            List<OffDayInfo> dummy = new List<OffDayInfo>();
            dummy = ReadOffdayFile();

            return RemoveOffday( offday_name, dummy );
        }

        /// <summary>
        /// 休暇リストから休暇名称を指定して休暇情報を削除し、ファイルへ保存
        /// </summary>
        /// <param name="offday_name"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static bool RemoveOffday( string offday_name, List<OffDayInfo> lst )
        {
            bool ret = false;

            // リストから削除
            lst.RemoveAll( x => x.OffDayName == offday_name );

            // ファイルへ保存
            try
            {
                WriteOffdayFile( lst );
                ret = true;
            }
            catch ( Exception e )
            {
                // 例外
                Console.WriteLine( e.Message );
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// 休暇ファイルから休暇情報を指定して削除
        /// </summary>
        /// <param name="offday"></param>
        /// <returns></returns>
        public static bool RemoveOffday( OffDayInfo offday )
        {
            // ファイル読み出し
            List<OffDayInfo> dummy = new List<OffDayInfo>();
            dummy = ReadOffdayFile();

            return RemoveOffday( offday, dummy );
        }

        /// <summary>
        /// 休暇リストから休暇情報を指定して削除し、ファイルへ保存
        /// </summary>
        /// <param name="offday"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static bool RemoveOffday( OffDayInfo offday, List<OffDayInfo> lst )
        {
            return RemoveOffday( offday.Year, offday.Mon, offday.Day, lst );
        }

        #endregion 休暇情報の削除

        #region 休暇情報の更新

        /// <summary>
        /// ファイルの休暇情報を更新
        /// </summary>
        /// <param name="offday"></param>
        /// <returns></returns>
        public static bool UpdateOffday( OffDayInfo offday )
        {
            // ファイルから読み出し
            List<OffDayInfo> dummy = new List<OffDayInfo>();
            dummy = ReadOffdayFile();

            return UpdateOffday( offday, ref dummy );
        }

        /// <summary>
        /// 休暇リストの休暇情報を更新し、ファイルへ保存
        /// </summary>
        /// <param name="offday"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static bool UpdateOffday( OffDayInfo offday, ref List<OffDayInfo> lst )
        {
            bool ret = false;

            // 削除
            ret = RemoveOffday( offday, lst );
            if ( false == ret )
            {
                return false;
            }

            // 追加
            ret = AddOffday( offday, lst );
            if ( false == ret )
            {
                return false;
            }

            return ret;
        }

        #endregion 休暇情報の更新

        #region 休暇情報ファイル読み出し

        /// <summary>
        /// テキストから休暇情報を読み出し、リスト化して返す
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static List<OffDayInfo> ReadOffdayFile( string FileName = DefaultFileName )
        {
            List<OffDayInfo> lst = new List<OffDayInfo>();
            StreamReader sr = null;

            // ファイルがあれば読み出す
            // なければリストを初期化して返す
            try
            {
                if ( false == File.Exists( FileName ) )
                {
                    return lst;
                }

                sr = new StreamReader( FileName, Encoding.Default );

                // コメント行を省いて取得する
                var lines = File.ReadLines( FileName, Encoding.Default )
                                .Where( x => !x.StartsWith( '#' ) );

                foreach ( string line in lines )
                {
                    OffDayInfo offday = null;
                    // 取得行を休暇情報に変換する
                    ConvertStructOffdayLine( line, out offday );

                    if ( offday != null )
                    {
                        // 正常値なら休暇リストへ追加する
                        lst.Add( offday );
                    }
                }
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }
            finally
            {
                if ( sr != null ) sr.Close();
            }

            return lst;
        }

        #endregion 休暇情報ファイル読み出し

        // 内部メソッド

        #region 休暇情報ファイル保存

        /// <summary>
        /// 休暇リストをテキストへ保存する
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        private static bool WriteOffdayFile( IReadOnlyList<OffDayInfo> lst, string FileName = DefaultFileName )
        {
            bool ret = false;

            // 既存ファイルがあれば、内容を全削除する
            // なければファイルを作成する
            using ( StreamWriter sw = new StreamWriter( FileName, false, Encoding.Default ) )
            {
                // メモを書き出す
                sw.WriteLine( "# [年],[月],[日],[休暇タイプ],[休暇名称],[備考]" );
                sw.WriteLine( "# 休暇タイプ：" );
                sw.WriteLine( "#   1：祝日" );
                sw.WriteLine( "#   2：振替休日" );
                sw.WriteLine( "#   3：個人的な休暇" );
                sw.WriteLine( "" );

                // リストの内容を書き出す
                if ( 0 == lst.Count )
                {
                    sw.WriteLine( "" );
                }
                else
                {
                    foreach ( OffDayInfo offday in lst )
                    {
                        sw.WriteLine( $"{offday.Year},{offday.Mon},{offday.Day},{offday.OffDayTypeNum},{offday.OffDayName},{offday.Biko}" );
                    }
                }
                ret = true;
            }

            return ret;
        }

        #endregion 休暇情報ファイル保存

        #region 休暇情報ファイル読み出し行の休暇情報構造体変換
        /// <summary>
        /// 読み出し行の休暇情報構造体変換
        /// </summary>
        /// <param name="line"></param>
        /// <param name="offday"></param>
        /// <returns></returns>
        private static bool ConvertStructOffdayLine( string line, out OffDayInfo offday )
        {
            // 年,月,日,休暇タイプ(数字),休暇名称,備考

            bool ret = false;
            offday = null;

            if ( false == string.IsNullOrEmpty( line ) )
            {
                string[] word = line.Split( ',' );
                if ( word.Length > 5 )
                {
                    int year = 0;
                    int mon = 0;
                    int day = 0;
                    int offdayTypeNum = 0;
                    string offdayName = "";
                    string biko = "";

                    Int32.TryParse( word[ 3 ], out offdayTypeNum );

                    if ( false == Int32.TryParse( word[ 0 ], out year )
                        || false == Int32.TryParse( word[ 1 ], out mon )
                        || false == Int32.TryParse( word[ 2 ], out day )
                        || Enum.GetNames( typeof( enmOffDayType ) ).Length <= offdayTypeNum )
                    {
                        return false;
                    }

                    offdayName = word[ 4 ];
                    if ( word.Length == 6 ) biko = word[ 5 ];

                    ret = true;
                    offday = new OffDayInfo( year, mon, day, offdayTypeNum, offdayName, biko );
                }
            }

            return ret;
        }
        #endregion 休暇情報ファイル読み出し行の休暇情報構造体変換

        #region リストから祝日情報読み出し
        /// <summary>
        /// リストから祝日情報を読み出す
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private static List<OffDayInfo> ExtractHoliday( List<OffDayInfo> lst )
        {
            List<OffDayInfo> offdayList = new List<OffDayInfo>();

            offdayList = lst.Where( 
                ( x ) => x.OffDayType == enmOffDayType.Holiday
                ).ToList();

            return offdayList;
        }
        #endregion リストから祝日情報読み出し

        #region リストから個人休暇情報読み出し
        /// <summary>
        /// リストから個人休暇情報を読み出す
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private static List<OffDayInfo> ExtractOffday( List<OffDayInfo> lst )
        {
            List<OffDayInfo> offdayList = new List<OffDayInfo>();

            offdayList = lst.Where( ( x ) => x.OffDayType == enmOffDayType.OffWork ).ToList();

            return offdayList;
        }
        #endregion リストから個人休暇情報読み出し

        #region リストから通常日情報読み出し
        /// <summary>
        /// リストから通常日報を読み出す
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private static List<OffDayInfo> ExtractNormalDay( List<OffDayInfo> lst )
        {
            List<OffDayInfo> offdayList = new List<OffDayInfo>();

            offdayList = lst.Where( ( x ) => x.OffDayType == enmOffDayType.None ).ToList();

            return offdayList;
        }
        #endregion リストから通常日情報読み出し

        #region 祝日の差異確認

        /// <summary>
        /// 祝日の差異確認
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        private static List<OffDayInfo> HolidayCompare( List<OffDayInfo> t1, List<OffDayInfo> t2 )
        {
            List<OffDayInfo> ret = new List<OffDayInfo>();

            // 備考の反映
            foreach ( OffDayInfo t1Offday in t1 )
            {
                if ( Holiday.isExistOffday( t2, t1Offday ) )
                {
                    OffDayInfo t2Offday = t2.Find( x => x.Year == t1Offday.Year && x.Mon == t1Offday.Mon && x.Day == t1Offday.Day );
                    // 同一日が比較対象に含まれるとき
                    if ( t1Offday.Biko != t2Offday.Biko )
                    {
                        // 備考に差異があればファイル情報をリストに残す
                        ret.Add( t2Offday );
                    }
                    else
                    {
                        ret.Add( t1Offday );
                    }
                }
                else
                {
                    ret.Add( t1Offday );
                }
            }

            // 追加された祝日の繁栄
            foreach ( OffDayInfo t2Offday in t2 )
            {
                if ( Holiday.isExistOffday( t1, t2Offday ) == false )
                {
                    // 追加の祝日がある場合
                    if ( Holiday.isExistOffday( ret, t2Offday ) == false )
                    {
                        // 祝日を追加
                        ret.Add( t2Offday );
                    }
                }
            }

            return ret;
        }

        #endregion 祝日の差異確認

    }

    #endregion 休暇リストクラス

    #region 祝日設定クラス

    public static class Holiday
    {
        // 内部変数
        private static int Year;

        // 公開メソッド

        #region 年間の祝日設定

        /// <summary>
        /// 年間の祝日設定リストを返す
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static List<OffDayInfo> GetHolidayList( int year )
        {
            Year = year;
            List<OffDayInfo> lst = new List<OffDayInfo>();

            // 1月の祝日
            SetHoliday_1( lst );
            // 2月の祝日
            SetHoliday_2( lst );
            // 3月の祝日
            SetHoliday_3( lst );
            // 4月の祝日
            SetHoliday_4( lst );
            // 5月の祝日
            SetHoliday_5( lst );
            // 6月の祝日
            SetHoliday_6( lst );
            // 7月の祝日
            SetHoliday_7( lst );
            // 8月の祝日
            SetHoliday_8( lst );
            // 9月の祝日
            SetHoliday_9( lst );
            // 10月の祝日
            SetHoliday_10( lst );
            // 11月の祝日
            SetHoliday_11( lst );
            // 12月の祝日
            SetHoliday_12( lst );

            // 日付順ソート
            CalOffDay.OffDayListSort( lst );

            return lst;
        }

        #endregion 年間の祝日設定

        #region 振替休日の設定

        /// <summary>
        /// 祝日リストを受け取り、振替休日の設定を行う
        /// </summary>
        /// <param name="lst"></param>
        public static void SetSubHoliday( List<OffDayInfo> OffdayList )
        {
            // 祝日を抽出する
            List<OffDayInfo> lst = OffdayList.Where( x => x.OffDayType == enmOffDayType.Holiday ).ToList();

            // 追加用
            List<OffDayInfo> SubHolidayList = new List<OffDayInfo>();

            // 祝日が日曜の場合、直近の祝日でない日を休日とする
            foreach ( OffDayInfo offday in lst )
            {
                // 祝日の曜日取得
                int wday = Convert.ToInt32( new DateTime( offday.Year, offday.Mon, offday.Day ).DayOfWeek );
                if ( 0 == wday )
                {
                    // 振替休日の設定を行う
                    // 翌日を候補とする
                    OffDayInfo kouhobi = GetOffdayInfoAfterDays( offday, 1 );
                    while ( true )
                    {
                        // 候補日が祝日か確認する
                        if ( isExistOffday( lst, kouhobi, enmOffDayType.Holiday ) == false )
                        {
                            // 存在しない場合、候補日を振替休日とする
                            break;
                        }
                        // さらに翌日を候補日とする
                        kouhobi = GetOffdayInfoAfterDays( kouhobi, 1 );
                    }

                    // 候補日を登録
                    SubHolidayList.Add( kouhobi );
                }
            }

            // 振替休日チェック
            if ( 0 < SubHolidayList.Count )
            {
                foreach ( OffDayInfo offday in SubHolidayList )
                {
                    // 候補日がすでに祝日でないかチェック
                    if ( false == isExistOffday( OffdayList, offday, enmOffDayType.Holiday ) )
                    {
                        // 存在しない場合、振替休日を登録
                        OffdayList.Add( new OffDayInfo( offday.Year, offday.Mon, offday.Day, enmOffDayType.SubHoliday, "振替休日" ) );
                    }
                }
            }

            // 日付順ソート
            CalOffDay.OffDayListSort( OffdayList );
        }

        #endregion 振替休日の設定

        #region 国民の休日の設定

        /// <summary>
        /// 祝日リストを受け取り、国民の休日の設定を行う
        /// </summary>
        /// <param name="lst"></param>
        public static void SetExHoliday( List<OffDayInfo> OffdayList )
        {
            // 祝日を抽出したリスト
            List<OffDayInfo> lst = OffdayList.Where( x => x.OffDayType == enmOffDayType.Holiday ).ToList();

            // 追加用
            List<OffDayInfo> ExList = new List<OffDayInfo>();

            // 祝日と祝日に挟まれた平日は国民の休日とする
            for ( int i = 0; i < lst.Count; i++ )
            {
                OffDayInfo offDayBase = lst[ i ];

                // 二日後
                OffDayInfo offDayNext = GetOffdayInfoAfterDays( offDayBase, 2 );
                if ( isExistOffday( lst, offDayNext, enmOffDayType.Holiday ) == false )
                {
                    // 二日後が祝日でなければ飛ばす
                    continue;
                }

                // 翌日
                OffDayInfo kouhobi = GetOffdayInfoAfterDays( offDayBase, 1 );
                if ( isExistOffday( lst, kouhobi, enmOffDayType.Holiday ) == true )
                {
                    // 翌日が祝日なら飛ばす
                    continue;
                }

                ExList.Add( kouhobi );
            }

            // 国民の休日チェック
            if ( 0 < ExList.Count )
            {
                foreach ( OffDayInfo offday in ExList )
                {
                    // 候補日がすでに祝日でないかチェック
                    if ( isExistOffday( OffdayList, offday, enmOffDayType.Holiday ) == false )
                    {
                        // 存在しない場合、国民の休日を登録
                        OffdayList.Add( new OffDayInfo( offday.Year, offday.Mon, offday.Day, enmOffDayType.ExHoliday, "国民の休日" ) );
                    }
                }
            }

            // 日付順ソート
            CalOffDay.OffDayListSort( OffdayList );
        }

        #endregion 国民の休日の設定

        #region 指定休暇日の指定日後を返す

        /// <summary>
        /// 指定休暇日の指定日後を返す
        /// </summary>
        /// <param name="offday"></param>
        /// <param name="afterDays"></param>
        /// <returns></returns>
        public static OffDayInfo GetOffdayInfoAfterDays( OffDayInfo offday, int afterDays )
        {
            int year = offday.Year;
            int mon = offday.Mon;
            int day = offday.Day;

            day += afterDays;
            if ( DateTime.DaysInMonth( year, mon ) < day )
            {
                // 日が末日を超えたら翌月に変更
                day = 1;
                mon++;
                if ( 12 < mon )
                {
                    // 12月を超えたら翌年に変更
                    year++;
                    mon = 1;
                }
            }

            return new OffDayInfo( year, mon, day, enmOffDayType.None, "" );
        }

        #endregion 指定休暇日の指定日後を返す

        #region 休暇リストに指定休暇情報が含まれるか調べる

        /// <summary>
        /// 休暇リストに指定休暇情報が含まれるか調べる
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="offday"></param>
        /// <returns></returns>
        public static bool isExistOffday( List<OffDayInfo> lst, OffDayInfo offday )
        {
            return isExistOffday( lst, offday.Year, offday.Mon, offday.Day );
        }

        public static bool isExistOffday( List<OffDayInfo> lst, int year, int mon, int day )
        {
            return lst.Exists( x => x.Year == year && x.Mon == mon && x.Day == day );
        }

        /// <summary>
        /// 休暇リストに指定休暇情報が含まれるか調べる
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="offday"></param>
        /// <param name="offdayType"></param>
        /// <returns></returns>
        public static bool isExistOffday( List<OffDayInfo> lst, OffDayInfo offday, enmOffDayType offdayType )
        {
            return isExistOffday( lst, offday.Year, offday.Mon, offday.Day, offdayType );
        }

        public static bool isExistOffday( List<OffDayInfo> lst, int year, int mon, int day, enmOffDayType offdayType )
        {
            return lst.Exists( x => x.Year == year && x.Mon == mon && x.Day == day && x.OffDayType == offdayType );
        }

        #endregion 休暇リストに指定休暇情報が含まれるか調べる

        // 内部メソッド

        #region 各月の祝日設定

        /// <summary>
        /// 1月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_1( List<OffDayInfo> lst )
        {
            // [元日] 1月1日
            lst.Add( new OffDayInfo( Year, 1, 1, enmOffDayType.Holiday, "元日" ) );

            // [成人の日] 1月の第2月曜日
            int MonFirstWDay = Convert.ToInt32( new DateTime( Year, 1, 1 ).DayOfWeek );
            int Day = 1 + ( ( 8 - MonFirstWDay ) % 7 ) + ( 7 * 1 );
            lst.Add( new OffDayInfo( Year, 1, Day, enmOffDayType.Holiday, "成人の日" ) );
        }

        /// <summary>
        /// 2月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_2( List<OffDayInfo> lst )
        {
            // [建国記念の日] 政令で定める日
            lst.Add( new OffDayInfo( Year, 2, 11, enmOffDayType.Holiday, "建国記念の日" ) );

            // [天皇誕生日] 2月23日
            lst.Add( new OffDayInfo( Year, 2, 23, enmOffDayType.Holiday, "天皇誕生日" ) );
        }

        /// <summary>
        /// 3月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_3( List<OffDayInfo> lst )
        {
            // [春分の日] 春分日
            int d = GetEquinox( Year, 21.4471M, 0.242377M );
            lst.Add( new OffDayInfo( Year, 3, d, enmOffDayType.Holiday, "春分の日" ) );
        }

        /// <summary>
        /// 4月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_4( List<OffDayInfo> lst )
        {
            // [昭和の日] 4月29日
            lst.Add( new OffDayInfo( Year, 4, 29, enmOffDayType.Holiday, "昭和の日" ) );
        }

        /// <summary>
        /// 5月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_5( List<OffDayInfo> lst )
        {
            // [憲法記念日] 5月3日
            lst.Add( new OffDayInfo( Year, 5, 3, enmOffDayType.Holiday, "憲法記念日" ) );

            // [みどりの日] 5月4日
            lst.Add( new OffDayInfo( Year, 5, 4, enmOffDayType.Holiday, "みどりの日" ) );

            // [こどもの日] 5月5日
            lst.Add( new OffDayInfo( Year, 5, 5, enmOffDayType.Holiday, "こどもの日" ) );

            if (Year == 2019)
            {
                // 天皇の即位
                lst.Add( new OffDayInfo( Year, 5, 1, enmOffDayType.Holiday, "天皇の即位" ) );
            }
        }

        /// <summary>
        /// 6月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_6( List<OffDayInfo> lst )
        {
            // 祝日なし
        }

        /// <summary>
        /// 7月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_7( List<OffDayInfo> lst )
        {
            // 令和２年（2020年）に限り、「海の日」は7月23日に、「スポーツの日」は7月24日になります。

            if ( 2020 == Year )
            {
                lst.Add( new OffDayInfo( Year, 7, 23, enmOffDayType.Holiday, "海の日" ) );
            }
            else
            {
                // [海の日] 7月の第3月曜日
                int MonFirstWDay = Convert.ToInt32( new DateTime( Year, 7, 1 ).DayOfWeek );
                int d = 1 + ( ( 8 - MonFirstWDay ) % 7 ) + ( 7 * 2 );
                lst.Add( new OffDayInfo( Year, 7, d, enmOffDayType.Holiday, "海の日" ) );
            }

            if ( 2020 == Year )
            {
                lst.Add( new OffDayInfo( Year, 7, 24, enmOffDayType.Holiday, "スポーツの日" ) );
            }
        }

        /// <summary>
        /// 8月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_8( List<OffDayInfo> lst )
        {
            // 令和２年（2020年）に限り、「山の日」は8月10日になります。

            if ( 2020 == Year )
            {
                lst.Add( new OffDayInfo( Year, 8, 10, enmOffDayType.Holiday, "山の日" ) );
            }
            else
            {
                // [山の日] 8月11日
                lst.Add( new OffDayInfo( Year, 8, 11, enmOffDayType.Holiday, "山の日" ) );
            }
        }

        /// <summary>
        /// 9月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_9( List<OffDayInfo> lst )
        {
            int Day;

            // [敬老の日] 9月の第3月曜日
            int MonFirstWDay = Convert.ToInt32( new DateTime( Year, 9, 1 ).DayOfWeek );
            Day = 1 + ( ( 8 - MonFirstWDay ) % 7 ) + ( 7 * 2 );
            lst.Add( new OffDayInfo( Year, 9, Day, enmOffDayType.Holiday, "敬老の日" ) );

            // [秋分の日] 秋分日
            Day = GetEquinox( Year, 23.8896M, 0.242032M );
            lst.Add( new OffDayInfo( Year, 9, Day, enmOffDayType.Holiday, "秋分の日" ) );
        }

        /// <summary>
        /// 10月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_10( List<OffDayInfo> lst )
        {
            // [スポーツの日] 10月の第2月曜日
            string OffdayName;
            if ( Year >= 2020 )
            {
                OffdayName = "スポーツの日";
            }
            else
            {
                OffdayName = "体育の日";
            }
            int MonFirstWDay = Convert.ToInt32( new DateTime( Year, 10, 1 ).DayOfWeek );
            int Day = 1 + ( ( 8 - MonFirstWDay ) % 7 ) + ( 7 * 1 );
            lst.Add( new OffDayInfo( Year, 10, Day, enmOffDayType.Holiday, OffdayName ) );

            if (Year == 2019)
            {
                // 即位礼正殿の儀
                lst.Add( new OffDayInfo( Year, 10, 22, enmOffDayType.Holiday, "即位礼正殿の儀" ) );
            }
        }

        /// <summary>
        /// 11月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_11( List<OffDayInfo> lst )
        {
            // [文化の日] 11月3日
            lst.Add( new OffDayInfo( Year, 11, 3, enmOffDayType.Holiday, "文化の日" ) );

            // [勤労感謝の日] 11月23日
            lst.Add( new OffDayInfo( Year, 11, 23, enmOffDayType.Holiday, "勤労感謝の日" ) );
        }

        /// <summary>
        /// 12月の祝日設定
        /// </summary>
        /// <param name="lst"></param>
        private static void SetHoliday_12( List<OffDayInfo> lst )
        {
            // なし
        }

        /// <summary>
        /// 春分の日 秋分の日 算出式
        /// </summary>
        /// <param name="year"></param>
        /// <param name="numA"></param>
        /// <param name="numB"></param>
        /// <returns></returns>
        private static int GetEquinox( int year, decimal numA, decimal numB )
        {
            Decimal res = Math.Floor( numA + ( numB * ( year - 1900 ) ) - Math.Floor( ( year - 1900 ) / 4.0M ) );
            return Convert.ToInt32( res );
        }

        #endregion 各月の祝日設定
    }

    #endregion 祝日判定クラス

}
