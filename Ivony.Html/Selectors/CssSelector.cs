﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections;
using Ivony.Fluent;
using System.Diagnostics;
using System.Globalization;

namespace Ivony.Html
{

  /// <summary>
  /// 提供一系列静态和扩展方法来辅助使用CSS选择器。
  /// </summary>
  public static class CssSelector
  {


    private static readonly Regex cssSelectorRegex = new Regex( "^" + Regulars.cssSelectorPattern + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant );


    /// <summary>
    /// 创建一个CSS选择器
    /// </summary>
    /// <param name="expression">选择器表达式</param>
    /// <returns></returns>
    public static ICssSelector Create( string expression )
    {
      if ( expression == null )
        throw new ArgumentNullException( "expression" );

      return Create( expression, null );
    }




    /// <summary>
    /// 创建一个CSS选择器
    /// </summary>
    /// <param name="expression">选择器表达式</param>
    /// <param name="scope">范畴限定</param>
    /// <returns></returns>
    public static ICssSelector Create( string expression, IHtmlContainer scope )
    {
      if ( expression == null )
        throw new ArgumentNullException( "expression" );

      Trace.TraceInformation( "Begin match expression" );
      var match = cssSelectorRegex.Match( expression );
      Trace.TraceInformation( "End match expression" );

      if ( !match.Success )
        throw new FormatException( "无法识别的CSS选择器" );


      Trace.TraceInformation( "Begin create casecading selectors" );
      var selectors = match.Groups["selector"].Captures.Cast<Capture>().Select( c => CssCasecadingSelector.Create( c.Value, scope ) ).ToArray();
      Trace.TraceInformation( "End create casecading selectors" );

      return new CssMultipleSelector( selectors );
    }



    /// <summary>
    /// 执行CSS选择器搜索
    /// </summary>
    /// <param name="expression">CSS选择器表达式</param>
    /// <param name="scope">CSS选择器和搜索范畴</param>
    /// <returns>搜索结果</returns>
    public static IEnumerable<IHtmlElement> Search( string expression, IHtmlContainer scope )
    {

      if ( expression == null )
        throw new ArgumentNullException( "expression" );

      if ( scope == null )
        throw new ArgumentNullException( "scope" );


      try
      {
        Trace.TraceInformation( "Create Selector" );
        var selector = Create( expression, scope );
        Trace.TraceInformation( "Create Selector completed" );
        return selector.Filter( scope.Descendants() );
      }
      catch ( Exception e )
      {
        if ( e.Data != null && !e.Data.Contains( "Ivony.Html.CssQuery.Expression" ) )
          e.Data["selector expression"] = expression;

        throw;
      }
    }

    /// <summary>
    /// 使用选择器从元素集中筛选出符合选择器要求的元素
    /// </summary>
    /// <param name="selector">选择器</param>
    /// <param name="source">源元素集</param>
    /// <returns>筛选结果</returns>
    public static IEnumerable<IHtmlElement> Filter( this ICssSelector selector, IEnumerable<IHtmlElement> source )
    {
      return source.Where( e => selector.IsEligible( e ) );
    }



    /// <summary>
    /// 调用此方法预热选择器
    /// </summary>
    public static void WarmUp()
    {
      cssSelectorRegex.Match( "" );
      CssCasecadingSelector.WarmUp();
    }


  }


}
