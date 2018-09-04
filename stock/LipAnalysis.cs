using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    /*
     * LipAnalysis 類別用來做絕對低點(Absolute Lowest Index Point)分析用，
     * 此分析法會試著找出股價的每波最高(x 日股價 maxIndex)及最低點(y 日股價minIndex)
     * ，並利用此資訊找出股價最大差值(下跌、上漲幅度)、y-x 的最大值、最小值及平均，
     * x-y 的最大值、最小值及平均。
     * 最後目標是找到股價位於最低檔並開始反轉的時機，分析用此方法找到的結果上漲及
     * 下跌的機率為何？
     * 若此方法可行則把找到的股票加入追踪系統。
     * 預期此方法應該適用長線的股價波動，應該很準，所以主要是以月線資料為分析以據，
     * 完成整個分析方法後，試試看對短線(日線)的分析結果為何。
     */
    class LipAnalysis
    {

    }
}
