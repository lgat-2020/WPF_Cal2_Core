using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text;
using System.Windows.Controls;
using CSL_Cal2_Core;
using System.Linq;
using System.Drawing;
using System.Windows;

namespace WPF_Cal2_Core
{
    public class AddOffday
    {
        #region フィールド

        private CalData calData;

        public enum OffdayTypeIndex : int
        {
            None = 0,
            Offday,
            Holiday,
        }

        private Dictionary<int, string> OffdayTypeList = new Dictionary<int, string>()
        {
            { (int)OffdayTypeIndex.None, "なし" },
            { (int)OffdayTypeIndex.Offday, "休暇" },
            { (int)OffdayTypeIndex.Holiday, "祝日" },
        };

        #endregion フィールド

        #region プロパティ

        public int Year { get; private set; }
        public int Mon { get; private set; }
        public int Day { get; private set; }

        public OffdayDate offdayDate { get; private set; }
        public OffdatType offdatType { get; private set; }
        public OffdayName offdayName { get; private set; }
        public OffdayBiko offdayBiko { get; private set; }

        #endregion プロパティ

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dt"></param>
        public AddOffday( CSL_Cal2_Core.CalData cal, DateTime dt ) : this( cal, dt.Year, dt.Month, dt.Day )
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <param name="day"></param>
        public AddOffday( CSL_Cal2_Core.CalData cal, int year, int mon, int day )
        {
            calData = cal;

            Year = year;
            Mon = mon;
            Day = day;

            offdayDate = new OffdayDate();
            offdatType = new OffdatType();
            offdayName = new OffdayName();
            offdayBiko = new OffdayBiko();
        }

        #endregion コンストラクタ

        #region 初期化

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            // 日付
            offdayDate.DateText.Value = "";

            // 種別
            offdatType.offdayType.Value = -1;

            // 名称
            offdayName.NameText.Value = "";

            // 備考
            offdayBiko.BikoText.Value = "";
        }

        #endregion 初期化

        #region 種別リスト作成

        /// <summary>
        /// 種別リスト作成
        /// </summary>
        /// <returns></returns>
        public List<string> CreateOffdayTypes()
        {
            List<string> Items = new List<string>();

            foreach ( KeyValuePair<int, string> kvp in OffdayTypeList )
            {
                Items.Add( kvp.Value );
            }

            return Items;
        }

        #endregion 種別リスト作成

        #region 指定日の休暇情報の取得

        // 指定日の休暇情報の取得
        private OffDayInfo GetOffdayInfo( int year, int mon, int day )
        {
            List<OffDayInfo> OffdayList = ReadOffdayList();
            return OffdayList.Find( x => x.Year == year && x.Mon == mon && x.Day == day );
        }

        #endregion 指定日の休暇情報の取得

        #region 指定日の休暇情報を反映する

        /// <summary>
        /// 指定日の休暇情報を反映する
        /// </summary>
        public void SetOffdayInfo()
        {
            // 対象日設定
            offdayDate.DateText.Value = Year.ToString() + "/" + Mon.ToString() + "/" + Day.ToString();

            // 休暇情報取得
            OffDayInfo offday = GetOffdayInfo( Year, Mon, Day );

            if ( offday == null )
            {
                // 指定休暇日がリストにないとき
                offdatType.offdayType.Value = ( int )OffdayTypeIndex.None;
            }
            else
            {
                // 指定休暇日がリストにあるとき

                // 種別設定
                switch ( offday.OffDayType )
                {
                    case enmOffDayType.Holiday:
                    case enmOffDayType.SubHoliday:
                    case enmOffDayType.ExHoliday:
                        offdatType.offdayType.Value = ( int )OffdayTypeIndex.Holiday;
                        break;

                    case enmOffDayType.OffWork:
                        offdatType.offdayType.Value = ( int )OffdayTypeIndex.Offday;
                        break;

                    case enmOffDayType.None:
                    default:
                        offdatType.offdayType.Value = ( int )OffdayTypeIndex.None;
                        break;
                }

                // 休暇名称
                offdayName.NameText.Value = offday.OffDayName;

                // 備考
                offdayBiko.BikoText.Value = offday.Biko;
            }
        }

        #endregion 指定日の休暇情報を反映する

        #region 休暇情報読み込み

        /// <summary>
        /// 休暇情報読み込み
        /// </summary>
        /// <returns></returns>
        private List<OffDayInfo> ReadOffdayList()
        {
            List<OffDayInfo> lst = CalOffDay.ReadOffdayFile();
            return lst;
        }

        #endregion 休暇情報読み込み

        #region 休暇種別変更時処理

        /// <summary>
        /// 休暇種別変更時処理
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool OffdayTypeChanged( int index )
        {
            // 祝日が選択された場合、無効
            if ( index == ( int )OffdayTypeIndex.Holiday )
            {
                return false;
            }

            offdatType.offdayType.Value = index;

            if ( index == ( int )OffdayTypeIndex.None )
            {
                offdayName.NameText.Value = "";
            }

            return true;
        }

        #endregion 休暇種別変更時処理

        #region 休暇情報更新

        /// <summary>
        /// 休暇情報更新
        /// </summary>
        public void OffdayInfoUpdate()
        {
            enmOffDayType type = enmOffDayType.None;
            string name = offdayName.NameText.Value;
            string biko = offdayBiko.BikoText.Value;

            if ( offdatType.offdayType.Value == (int)OffdayTypeIndex.Holiday )
            {
                OffDayInfo _tmp = GetOffdayInfo( Year, Mon, Day );
                type = _tmp.OffDayType;
            }
            else
            {
                type = ( offdatType.offdayType.Value == ( int )OffdayTypeIndex.Offday ) ? enmOffDayType.OffWork : enmOffDayType.None;
            }

            OffDayInfo offday = new OffDayInfo( Year, Mon, Day, type, name, biko );

            if ( CalOffDay.UpdateOffday( offday ) == false )
            {
                MessageBox.Show( "休暇リストの更新に失敗しました" );
            }
        }

        #endregion 休暇情報更新
    }

    public class OffdayDate
    {
        public ReactiveProperty<string> DateText { get; private set; }

        public OffdayDate()
        {
            DateText = new ReactiveProperty<string>();
        }
    }

    public class OffdatType
    {
        public ReactiveProperty<int> offdayType { get; set; }

        public OffdatType()
        {
            offdayType = new ReactiveProperty<int>();
        }
    }

    public class OffdayName
    {
        public ReactiveProperty<string> NameText { get; set; }

        public OffdayName()
        {
            NameText = new ReactiveProperty<string>();
        }
    }

    public class OffdayBiko
    {
        public ReactiveProperty<string> BikoText { get; set; }

        public OffdayBiko()
        {
            BikoText = new ReactiveProperty<string>();
        }
    }
}
