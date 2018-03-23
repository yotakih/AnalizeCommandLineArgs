using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AnalyzeCommandLineArgs
{

  public class CommandLineAnalyzer
  {
    private List<CommandOption> _optionList;
    private CommandOption _helpOption;

    public CommandLineAnalyzer()
    {
      this._optionList = new List<CommandOption>();
      this._helpOption = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="option"></param>
    public void addCommandOption(CommandOption option)
    {
      this._optionList.Add(option);
    }

    public CommandOption helpOption
    {
      set { this._helpOption = value; }
    }

    /// <summary>
    /// コマンドライン引数の解析とチェックを行います
    /// </summary>
    public bool analyze()
    {
      string[] args = Environment.GetCommandLineArgs();
      //ヘルプ表示オプションのチェック
      if (this._helpOption != null)
      {
        if (args.Length > 1)
        {
          if (args[1] == this._helpOption.simbol)
          {
            writeLineDetail();
            return false;
          }
        }
      }

      try
      {
        for (int i = 1; i < args.Length; i++)
        {
          CommandOption hitOp = null;
          foreach (CommandOption op in this._optionList)
          {
            if (args[i] == op.simbol) hitOp = op;
          }
          //存在しないオプションでないか確認
          if (hitOp == null) throw new Exception(string.Format(@"{0}は不正なオプションです。", args[i]));
          //すでに登録されていないか確認
          if (hitOp.selected) throw new Exception(string.Format(@"{0}が重複しています。", args[i]));
          //オプションが指定されたことを登録
          hitOp.selected = true;
          //オプションに対する引数の取得
          if (!hitOp.requiredArgflg) continue;
          if (!(i + 1 < args.Length)) throw new Exception(string.Format(@"{0}の引数がありません。", args[i]));
          hitOp.arg = args[i + 1];
          i += 1;
        }

        checkCommandOptions();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        return false;
      }
      return true;
    }

    private void checkCommandOptions()
    {
      foreach (CommandOption op in this._optionList)
      {
        if (op.requiredFlg & !op.selected) throw new Exception(string.Format(@"指定必須オプション{0}がありません。", op.simbol));
        if (!op.selected) continue;
        //排他オプションの確認
        foreach (CommandOption exclusionOp in op.exclusionOptionList)
        {
          if (exclusionOp.selected) throw new Exception(string.Format(@"{0}と{1}は同時に指定できません。", op.simbol, exclusionOp.simbol));
        }
        //協調オプションの確認
        foreach (CommandOption coopareteOp in op.coopareteOptionList)
        {
          if (!coopareteOp.selected) throw new Exception(string.Format(@"{0}には{1}の指定が必要です。", op.simbol, coopareteOp.simbol));
        }
        //引数チェックfunctionの実行
        if (op.checkArgAction != null)
        {
          op.checkArgAction.Invoke(op.arg);
        }
      }
    }

    public void writeLineDetail()
    {
      Console.WriteLine();
      foreach (CommandOption op in this._optionList)
      {
        Console.WriteLine(op.detail);
      }
    }

  }

  public class CommandOption
  {
    private string _simbol;
    public bool selected;
    public string detail;
    public Action<string> checkArgAction;
    public bool requiredFlg;                         /*必須オプションか否か*/
    private string _arg;
    public bool requiredArgflg;                      /*引数をとるか否か*/
    public List<CommandOption> exclusionOptionList;
    public List<CommandOption> coopareteOptionList;

    public CommandOption(string simbol, bool requiredFlg, bool requiredArgFlg, string detail, Action<string> checkAction)
    {
      this._simbol = simbol;
      this.selected = false;
      this.requiredFlg = requiredFlg;
      this.requiredArgflg = requiredArgFlg;
      this.detail = detail;
      this.checkArgAction = checkAction;
      this.exclusionOptionList = new List<CommandOption>();
      this.coopareteOptionList = new List<CommandOption>();
    }

    public string simbol
    {
      get { return this._simbol; }
      set { this._simbol = value; }
    }

    public string arg
    {
      get { return this._arg; }
      set { this._arg = value; }
    }
  }

}