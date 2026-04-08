using System;
using System.Collections.Generic;
using System.Threading;
namespace LifeGameDemo
{
    class Choice
    {
        public string Text, Result, Achievement;
        public int GoldChange;
        public string ActionType; // help/harm/selfish/selfless/ignore/neutral
        public Choice(string t, string r, string act, int g = 0, string a = null)
        { Text = t; Result = r; ActionType = act; GoldChange = g; Achievement = a; }
    }
    class GridEvent
    {
        public int Age, GoldReward;
        public string Title, Description;
        public bool HasDeathRisk;
        public double DeathChance;
        public Choice[] Choices;
        public GridEvent(int a, string t, string d, int g, bool dr, double dc, Choice[] c = null)
        { Age = a; Title = t; Description = d; GoldReward = g; HasDeathRisk = dr; DeathChance = dc; Choices = c; }
    }
    class Program
    {
        static Random R = new Random();
        static int age, gold, hiddenKarma, gender, fw, reinc, nsc;
        static string pname = "", ft = "", ds = "";
        static bool dead;
        static List<string> achs = new List<string>();
        static List<string> smem = new List<string>();
        static List<GridEvent> grids;
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "LifeGame - \u4eba\u751f\u6a21\u62df\u5668";
            while (true) Menu();
        }
        static void Menu()
        {
            Console.Clear(); Console.ForegroundColor = ConsoleColor.Cyan; W("");
            Ctr("+===================================+");
            Ctr("|     L I F E   G A M E            |");
            Ctr("|     \u4eba \u751f \u6a21 \u62df \u5668                |");
            Ctr("+===================================+"); Console.ResetColor();
            if (reinc > 0) { Console.ForegroundColor = ConsoleColor.Magenta; Ctr("[ \u7b2c " + (reinc+1) + " \u4e16 | \u7075\u9b42\u8bb0\u5fc6: " + smem.Count + "\u6761 ]"); Console.ResetColor(); }
            W(""); Ctr("1. \u5f00\u59cb\u65b0\u4eba\u751f"); Ctr("2. \u9000\u51fa\u6e38\u620f"); W("\n  > ");
            if (Console.ReadKey(true).KeyChar == '1') NewLife(); else Environment.Exit(0);
        }
        static void NewLife()
        {
            age=0; gold=0; hiddenKarma=0; ft=""; fw=0; ds=""; nsc=20; dead=false;
            achs.Clear(); grids = MakeGrids();
            Console.Clear(); Box("\u89d2\u8272\u521b\u5efa"); W("");
            Console.Write("  \u4f60\u7684\u540d\u5b57: "); pname = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(pname)) pname = "\u65e0\u540d\u65c5\u4eba";
            W("\n  \u9009\u62e9\u6027\u522b:\n  1. \u7537\n  2. \u5973");
            Console.Write("  > "); gender = Console.ReadKey(true).KeyChar=='2'?1:0;
            Console.WriteLine(gender==0?"\u7537":"\u5973"); PickSpeed();
            Console.Clear(); Box("\u51fa\u751f"); W("");
            Tw("  " + pname + "\uff0c\u4f60\u6765\u5230\u4e86\u8fd9\u4e2a\u4e16\u754c\u3002");
            Tw("  \u4e00\u58f0\u557c\u54ed\uff0c\u4e00\u6bb5\u4eba\u751f\u5c31\u6b64\u5f00\u59cb...");
            if (smem.Count > 0) { Console.ForegroundColor = ConsoleColor.Magenta; Tw("  \u4f60\u9690\u7ea6\u611f\u5230\u4e00\u4e9b\u6a21\u7cca\u7684\u8bb0\u5fc6..."); Console.ResetColor(); }
            Wait(); Loop();
        }
        static void Loop()
        {
            while (!dead && age <= 100)
            {
                Console.Clear(); Bar(); W(""); Box("\u684c\u6e38\u5c42 - " + Phase(age)); W("");
                if (age >= nsc) { PickSpeed(); nsc += 20; Console.Clear(); Bar(); W(""); Box("\u684c\u6e38\u5c42 - " + Phase(age)); W(""); }
                Console.ForegroundColor = ConsoleColor.Yellow; W("  \u6309 [\u7a7a\u683c] \u6447\u9ab0\u5b50..."); Console.ResetColor();
                while (Console.ReadKey(true).Key != ConsoleKey.Spacebar) {}
                int dice = Roll(); int tgt = Math.Min(age + dice, 100);
                for (int a = age+1; a <= tgt; a++)
                {
                    age = a;
                    if (a == 6 && fw == 0) GenFamily();
                    var g = Find(a);
                    if (g != null) { Enter(g); if (dead) return; }
                }
                if (age >= 100 && !dead) { Console.Clear(); Box("\u767e\u5c81\u4eba\u745e"); Tw("  " + pname + "\uff0c\u4f60\u6d3b\u8fc7\u4e86\u4e00\u4e2a\u4e16\u7eaa\u3002"); achs.Add("\u767e\u5c81\u4eba\u745e"); Wait(); dead = true; }
            }
            Death();
        }
        static void Enter(GridEvent g)
        {
            Console.Clear(); Bar(); W("");
            Console.ForegroundColor = PColor(g.Age); Box(g.Age + "\u5c81 - " + g.Title); Console.ResetColor(); W("");
            Tw("  " + g.Description); W("");
            if (g.HasDeathRisk)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed; W("  \u26a0 \u4f60\u611f\u5230\u4e00\u4e1d\u4e0d\u5b89..."); Console.ResetColor();
                if (R.NextDouble() < g.DeathChance)
                {
                    W(""); Console.ForegroundColor = ConsoleColor.Red; Tw("  ......"); Thread.Sleep(800);
                    Tw("  " + pname + "\uff0c\u4f60\u7684\u4eba\u751f\u5728" + g.Age + "\u5c81\u753b\u4e0a\u4e86\u53e5\u53f7\u3002");
                    Console.ResetColor(); dead=true; gold+=g.GoldReward; Wait(); return;
                }
                Console.ForegroundColor = ConsoleColor.Green; Tw("  \u4f60\u631a\u8fc7\u6765\u4e86\u3002"); Console.ResetColor();
            }
            W("");
            if (g.Choices != null && g.Choices.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan; W("  \u4f60\u9762\u4e34\u4e00\u4e2a\u9009\u62e9:"); Console.ResetColor(); W("");
                for (int i = 0; i < g.Choices.Length; i++)
                    W("  " + (i+1) + ". " + g.Choices[i].Text);
                Console.Write("\n  > ");
                int ch = 0;
                while (ch < 1 || ch > g.Choices.Length) { var k = Console.ReadKey(true).KeyChar; if (int.TryParse(k.ToString(), out ch) && ch>=1 && ch<=g.Choices.Length) Console.WriteLine(ch); else ch=0; }
                var s = g.Choices[ch-1]; W(""); Tw("  " + s.Result);
                // Hidden karma - player never sees this
                hiddenKarma += CalcKarma(s.ActionType);
                gold += s.GoldChange;
                if (!string.IsNullOrEmpty(s.Achievement)) { achs.Add(s.Achievement); Console.ForegroundColor=ConsoleColor.Magenta; W("\n  \u2605 \u8fbe\u6210\u6210\u5c31: " + s.Achievement); Console.ResetColor(); }
            }
            if (g.GoldReward > 0) { gold += g.GoldReward; Console.ForegroundColor=ConsoleColor.Yellow; W("\n  +" + g.GoldReward + " \u91d1\u5e01"); Console.ResetColor(); }
            Wait();
        }
        /// <summary>Hidden karma calculation - randomized per action type</summary>
        static int CalcKarma(string actionType)
        {
            switch (actionType)
            {
                case "help": return R.Next(1, 4);       // +1 to +3
                case "selfless": return R.Next(1, 3);   // +1 to +2
                case "harm": return R.Next(-3, 0);      // -3 to -1
                case "selfish": return R.Next(-2, 1);   // -2 to 0
                case "ignore": return R.Next(-2, 1);    // -2 to 0
                default: return R.Next(-1, 2);           // -1 to +1
            }
        }
        static void Death()
        {
            Console.Clear(); Console.ForegroundColor = ConsoleColor.DarkGray; W("\n");
            Ctr("\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501");
            Ctr("\u4eba \u751f \u7ec8 \u7ae0"); Ctr("\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501");
            Console.ResetColor(); W(""); Thread.Sleep(1000);
            Tw("  " + pname + "\uff0c" + (gender==0?"\u4ed6":"\u5979") + "\u7684\u4e00\u751f\u8d70\u5230\u4e86\u5c3d\u5934\u3002");
            Tw("  \u4eab\u5e74 " + age + " \u5c81\u3002"); W(""); Thread.Sleep(500);
            Tw("  \u4e00\u751f\u79ef\u7d2f: " + gold + " \u91d1\u5e01");
            if (achs.Count > 0) Tw("  \u6210\u5c31: " + string.Join(", ", achs));
            W(""); Thread.Sleep(1000);
            // Karma revealed as description only
            string karmaDesc;
            if (hiddenKarma > 10) karmaDesc = "\u4e00\u4e2a\u771f\u6b63\u5584\u826f\u7684\u7075\u9b42";
            else if (hiddenKarma > 5) karmaDesc = "\u5584\u826f\u591a\u4e8e\u81ea\u79c1";
            else if (hiddenKarma > 0) karmaDesc = "\u5fc3\u4e2d\u6709\u5149";
            else if (hiddenKarma == 0) karmaDesc = "\u5584\u6076\u6301\u5e73\uff0c\u5982\u540c\u5929\u79e4";
            else if (hiddenKarma > -5) karmaDesc = "\u5fc3\u4e2d\u6709\u4e9b\u9057\u61be";
            else if (hiddenKarma > -10) karmaDesc = "\u6709\u4e9b\u503a\u672a\u8fd8";
            else karmaDesc = "\u6c89\u91cd\u7684\u826f\u5fc3";
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Tw("  \u4e00\u751f\u8bc4\u4ef7: " + karmaDesc);
            Console.ResetColor(); W(""); Thread.Sleep(1000);
            string realm;
            if (hiddenKarma > 0) { realm="\u5929\u5802"; Console.ForegroundColor=ConsoleColor.Cyan; Tw("  \u7075\u9b42\u5347\u8d77\uff0c\u91d1\u5149\u7b3c\u7f69..."); Thread.Sleep(500); Tw("  \u4f60\u88ab\u5f15\u5411\u4e86\u5929\u5802\u3002"); }
            else if (hiddenKarma < 0) { realm="\u5730\u72f1"; Console.ForegroundColor=ConsoleColor.DarkRed; Tw("  \u9ed1\u6697\u5411\u4f60\u5ef6\u4f38..."); Thread.Sleep(500); Tw("  \u4f60\u5760\u5165\u4e86\u5730\u72f1\u3002"); }
            else { realm=R.Next(2)==0?"\u5929\u5802":"\u5730\u72f1"; Console.ForegroundColor=ConsoleColor.DarkYellow; Tw("  \u547d\u8fd0\u7684\u5929\u79e4\u6447\u6446\u4e0d\u5b9a..."); Thread.Sleep(500); Tw("  \u6700\u7ec8\uff0c\u4f60\u88ab\u9001\u5f80\u4e86" + realm + "\u3002"); }
            Console.ResetColor(); W("\n");
            if (realm == "\u5929\u5802") { Console.ForegroundColor=ConsoleColor.Cyan; Box("\u5929 \u5802"); Console.ResetColor(); Tw("  \u4e91\u6d77\u4e4b\u4e0a\uff0c\u5149\u8292\u4e07\u4e08\u3002"); Tw("  \u4f60\u611f\u5230\u524d\u6240\u672a\u6709\u7684\u5e73\u9759\u3002"); }
            else { Console.ForegroundColor=ConsoleColor.DarkRed; Box("\u5730 \u72f1"); Console.ResetColor(); Tw("  \u70c8\u7130\u4e0e\u9ed1\u6697\u4ea4\u7ec7\u3002"); Tw("  \u8fd9\u91cc\u662f\u8d4e\u7f6a\u4e4b\u5730\u3002"); }
            W(""); Thread.Sleep(1000);
            Console.ForegroundColor = ConsoleColor.White;
            Tw("  \u4e00\u4e2a\u58f0\u97f3\u5728\u8033\u8fb9\u54cd\u8d77\uff1a");
            Tw("  \"\u4f60\u613f\u610f\u8f6c\u4e16\u91cd\u6765\u5417\uff1f\"");
            W("\n  1. \u8f6c\u4e16\uff08\u4fdd\u7559\u7075\u9b42\u8bb0\u5fc6\uff09\n  2. \u56de\u5230\u4e3b\u83dc\u5355"); Console.ResetColor(); Console.Write("  > ");
            if (Console.ReadKey(true).KeyChar == '1')
            {
                smem.AddRange(achs); reinc++; W("");
                Console.ForegroundColor=ConsoleColor.Magenta;
                Tw("  \u5149\u8292\u95ea\u70c1\uff0c\u4f60\u7684\u610f\u8bc6\u9010\u6e10\u6a21\u7cca...");
                Tw("  \u5f53\u4f60\u518d\u6b21\u7741\u5f00\u773c\uff0c\u4e00\u5207\u91cd\u65b0\u5f00\u59cb\u3002");
                Console.ResetColor(); Thread.Sleep(1500); NewLife();
            }
        }
        static void PickSpeed()
        {
            Console.Clear(); Box("\u9009\u62e9\u4eba\u751f\u8282\u594f"); W("");
            W("  1. \u6162\uff08\u9ab0\u5b50 1-3\uff09- \u7ec6\u7ec6\u54c1\u5473");
            W("  2. \u5feb\uff08\u9ab0\u5b50 3-6\uff09- \u5feb\u901f\u63a8\u8fdb");
            Console.Write("  > "); ds = Console.ReadKey(true).KeyChar=='2'?"fast":"slow";
            Console.WriteLine(ds=="fast"?"\u5feb":"\u6162"); Thread.Sleep(500);
        }
        static int Roll()
        {
            W(""); for (int i=0;i<8;i++) { Console.Write("\r  [ " + R.Next(1,7) + " ]"); Thread.Sleep(100); }
            int mn=ds=="fast"?3:1, mx=ds=="fast"?7:4, r=R.Next(mn,mx);
            Console.Write("\r  [ " + r + " ]  ");
            Console.ForegroundColor=ConsoleColor.Green; W("\u524d\u8fdb " + r + " \u5e74!"); Console.ResetColor();
            Thread.Sleep(600); return r;
        }
        static void GenFamily()
        {
            string[] ts = {"\u4e66\u9999\u95e8\u7b2c","\u7ecf\u5546\u4e16\u5bb6","\u52a1\u519c\u4e4b\u5bb6","\u519b\u4eba\u5bb6\u5ead","\u827a\u672f\u4e16\u5bb6","\u533b\u5b66\u4e16\u5bb6","\u666e\u901a\u5bb6\u5ead","\u5355\u4eb2\u5bb6\u5ead","\u5bcc\u8c6a\u4e4b\u5bb6"};
            int[] wg = {50,200,500,1500,5000};
            double rv = R.NextDouble();
            fw = rv<0.15?1:rv<0.45?2:rv<0.75?3:rv<0.92?4:5;
            ft = ts[R.Next(ts.Length)]; gold = wg[fw-1];
            Console.Clear(); Box("6\u5c81 - \u5bb6\u5ead\u80cc\u666f\u63ed\u6653"); W("");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Tw("  \u5bb6\u5ead: " + ft);
            string stars = "", empty = "";
            for(int i=0;i<fw;i++) stars+="\u2605"; for(int i=0;i<5-fw;i++) empty+="\u2606";
            Tw("  \u8d22\u5bcc\u7b49\u7ea7: " + stars + empty);
            Tw("  \u521d\u59cb\u8d44\u91d1: " + gold + " \u91d1\u5e01");
            Console.ResetColor(); Wait();
        }
        static void Bar()
        {
            Console.ForegroundColor=ConsoleColor.DarkGray; W("  ------------------------------------------------");
            Console.ForegroundColor=PColor(age); Console.Write("  " + pname);
            Console.ForegroundColor=ConsoleColor.White; Console.Write(" | " + age + "\u5c81");
            Console.ForegroundColor=ConsoleColor.Yellow; Console.Write(" | $" + gold);
            Console.ForegroundColor=ConsoleColor.DarkGray; Console.Write(" | " + Phase(age));
            if (ft!="") Console.Write(" | " + ft);
            // NO karma shown - it's hidden!
            W(""); W("  ------------------------------------------------"); Console.ResetColor();
        }
        static string Phase(int a) => a<=12?"\u7ae5\u5e74":a<=17?"\u5c11\u5e74":a<=30?"\u9752\u5e74":a<=50?"\u58ee\u5e74":a<=65?"\u4e2d\u5e74":"\u8001\u5e74";
        static ConsoleColor PColor(int a) => a<=12?ConsoleColor.Cyan:a<=17?ConsoleColor.Green:a<=30?ConsoleColor.Yellow:a<=50?ConsoleColor.White:a<=65?ConsoleColor.DarkYellow:ConsoleColor.Gray;
        static void Tw(string t, int d=30) { foreach(char c in t){Console.Write(c);Thread.Sleep(d);} Console.WriteLine(); }
        static void W(string s) { Console.WriteLine(s); }
        static void Ctr(string t) { try{Console.WriteLine(new string(' ',Math.Max(0,(Console.WindowWidth-t.Length)/2))+t);}catch{Console.WriteLine(t);} }
        static void Box(string t) { W("  +------------------------------------+"); string p=t.PadLeft((36+t.Length)/2).PadRight(36); W("  |"+p+"|"); W("  +------------------------------------+"); }
        static void Wait() { W(""); Console.ForegroundColor=ConsoleColor.DarkGray; W("  \u6309\u4efb\u610f\u952e\u7ee7\u7eed..."); Console.ResetColor(); Console.ReadKey(true); }
        static void PrintBanner() { Console.ForegroundColor=ConsoleColor.Cyan; W(""); Ctr("+===================================+"); Ctr("|     L I F E   G A M E            |"); Ctr("|     \u4eba \u751f \u6a21 \u62df \u5668                |"); Ctr("+===================================+"); Console.ResetColor(); W(""); }
        static GridEvent Find(int a) { foreach(var g in grids) if(g.Age==a) return g; return null; }
        static List<GridEvent> MakeGrids()
        {
            return new List<GridEvent>
            {
                new GridEvent(1, "\u51fa\u751f", "\u4e00\u58f0\u557c\u54ed\uff0c\u4f60\u6765\u5230\u4e86\u8fd9\u4e2a\u4e16\u754c\u3002", 0, false, 0),
                new GridEvent(3, "\u7b2c\u4e00\u6b21\u8bf4\u8bdd", "\u4f60\u5b66\u4f1a\u4e86\u53eb\u5988\u5988\uff0c\u5168\u5bb6\u4eba\u90fd\u7b11\u4e86\u3002", 0, false, 0),
                new GridEvent(5, "\u5e7c\u513f\u56ed", "\u4f60\u4ea4\u5230\u4e86\u4eba\u751f\u4e2d\u7b2c\u4e00\u4e2a\u670b\u53cb\u3002", 5, false, 0, new[] {
                    new Choice("\u548c\u5c0f\u670b\u53cb\u5206\u4eab\u73a9\u5177", "\u4f60\u4eec\u6210\u4e86\u597d\u670b\u53cb\u3002", "help"),
                    new Choice("\u62a2\u8d70\u522b\u4eba\u7684\u73a9\u5177", "\u5c0f\u670b\u53cb\u54ed\u4e86\uff0c\u8001\u5e08\u6279\u8bc4\u4e86\u4f60\u3002", "harm") }),
                new GridEvent(7, "\u5c0f\u5b66\u4e00\u5e74\u7ea7", "\u80cc\u4e0a\u4e66\u5305\uff0c\u8d70\u8fdb\u6821\u56ed\u3002", 10, false, 0),
                new GridEvent(10, "\u5c11\u5e74\u5fc3\u4e8b", "\u4f60\u7b2c\u4e00\u6b21\u5bf9\u4e00\u4e2a\u540c\u5b66\u4ea7\u751f\u4e86\u7279\u522b\u7684\u611f\u89c9\u3002", 10, false, 0, new[] {
                    new Choice("\u5199\u4e86\u4e00\u5c01\u4fe1\u5077\u5077\u585e\u8fdbTA\u7684\u4e66\u5305", "TA\u770b\u5230\u540e\u8138\u7ea2\u4e86\uff0c\u51b2\u4f60\u7b11\u4e86\u7b11\u3002", "neutral", 0, "\u521d\u604b\u840c\u82bd"),
                    new Choice("\u628a\u8fd9\u4efd\u5fc3\u601d\u85cf\u5728\u5fc3\u5e95", "\u591a\u5e74\u540e\u4f60\u4f9d\u7136\u8bb0\u5f97\u90a3\u4e2a\u540d\u5b57\u3002", "neutral") }),
                new GridEvent(12, "\u5c0f\u5347\u521d", "\u7b2c\u4e00\u6b21\u9762\u5bf9\u4eba\u751f\u7684\u5206\u53c9\u8def\u53e3\u3002", 20, false, 0, new[] {
                    new Choice("\u52aa\u529b\u8003\u4e0a\u91cd\u70b9\u4e2d\u5b66", "\u4f60\u4ed8\u51fa\u4e86\u5f88\u591a\uff0c\u4f46\u4e5f\u6536\u83b7\u4e86\u6210\u957f\u3002", "selfless", 50),
                    new Choice("\u968f\u7f18\uff0c\u53bb\u4e86\u666e\u901a\u4e2d\u5b66", "\u8f7b\u677e\u7684\u65e5\u5b50\u4e5f\u6709\u8f7b\u677e\u7684\u5feb\u4e50\u3002", "neutral", 20) }),
                new GridEvent(14, "\u53db\u9006\u671f", "\u4f60\u5f00\u59cb\u8d28\u7591\u4e00\u5207\uff0c\u548c\u7236\u6bcd\u7684\u5173\u7cfb\u53d8\u5f97\u7d27\u5f20\u3002", 0, false, 0, new[] {
                    new Choice("\u548c\u7236\u6bcd\u5927\u5435\u4e00\u67b6\u540e\u79bb\u5bb6\u51fa\u8d70", "\u5728\u5916\u9762\u5f85\u4e86\u4e00\u665a\uff0c\u6700\u7ec8\u8fd8\u662f\u56de\u4e86\u5bb6\u3002", "harm"),
                    new Choice("\u867d\u7136\u4e0d\u7406\u89e3\uff0c\u4f46\u9009\u62e9\u4e86\u6c9f\u901a", "\u4f60\u4eec\u4e4b\u95f4\u7684\u5173\u7cfb\u53cd\u800c\u66f4\u8fd1\u4e86\u3002", "selfless", 0, "\u7406\u89e3\u4e07\u5c81") }),
                new GridEvent(15, "\u4e2d\u8003", "\u51b3\u5b9a\u4f60\u8fdb\u5165\u4ec0\u4e48\u6837\u7684\u9ad8\u4e2d\u3002", 30, false, 0),
                new GridEvent(18, "\u9ad8\u8003", "\u5343\u519b\u4e07\u9a6c\u8fc7\u72ec\u6728\u6865\u3002", 50, false, 0, new[] {
                    new Choice("\u8d85\u5e38\u53d1\u6325\uff0c\u8003\u4e0a\u4e86\u7406\u60f3\u5927\u5b66", "\u5f55\u53d6\u901a\u77e5\u4e66\u5230\u7684\u90a3\u5929\uff0c\u5988\u5988\u54ed\u4e86\u3002", "neutral", 100, "\u91d1\u699c\u9898\u540d"),
                    new Choice("\u53d1\u6325\u5931\u5e38\uff0c\u53bb\u4e86\u666e\u901a\u5927\u5b66", "\u4eba\u751f\u4e0d\u53ea\u6709\u4e00\u6761\u8def\u3002", "neutral", 30),
                    new Choice("\u653e\u5f03\u9ad8\u8003\uff0c\u76f4\u63a5\u6253\u5de5", "\u4f60\u6bd4\u540c\u9f84\u4eba\u66f4\u65e9\u89c1\u8bc6\u4e86\u793e\u4f1a\u3002", "neutral", 80, "\u793e\u4f1a\u5927\u5b66") }),                new GridEvent(20, "\u5927\u5b66\u65f6\u5149", "\u81ea\u7531\u7684\u7a7a\u6c14\uff0c\u65e0\u9650\u7684\u53ef\u80fd\u3002", 40, false, 0, new[] {
                    new Choice("\u6ce1\u56fe\u4e66\u9986\uff0c\u62ff\u5956\u5b66\u91d1", "\u56db\u5e74\u540e\u4f60\u4ee5\u4f18\u5f02\u6210\u7ee9\u6bd5\u4e1a\u3002", "selfless", 60),
                    new Choice("\u793e\u56e2\u6d3b\u52a8\uff0c\u5e7f\u4ea4\u670b\u53cb", "\u4f60\u7684\u4eba\u8109\u6210\u4e86\u672a\u6765\u6700\u5927\u7684\u8d22\u5bcc\u3002", "help", 20, "\u793e\u4ea4\u8fbe\u4eba"),
                    new Choice("\u6c89\u8ff7\u6e38\u620f\uff0c\u6df7\u65e5\u5b50", "\u5feb\u4e50\u662f\u771f\u7684\uff0c\u7a7a\u865a\u4e5f\u662f\u771f\u7684\u3002", "selfish") }),
                new GridEvent(22, "\u6bd5\u4e1a\u65c5\u884c", "\u548c\u670b\u53cb\u4eec\u6700\u540e\u7684\u72c2\u6b22\u3002", 30, false, 0),
                new GridEvent(24, "\u7b2c\u4e00\u4efd\u5de5\u4f5c", "\u8e0f\u5165\u793e\u4f1a\u7684\u7b2c\u4e00\u6b65\u3002", 100, false, 0, new[] {
                    new Choice("\u5728\u5927\u516c\u53f8\u4ece\u5e95\u5c42\u505a\u8d77", "\u52a0\u73ed\u662f\u5e38\u6001\uff0c\u4f46\u4f60\u5b66\u5230\u4e86\u5f88\u591a\u3002", "neutral", 150),
                    new Choice("\u52a0\u5165\u521b\u4e1a\u516c\u53f8", "\u9ad8\u98ce\u9669\u9ad8\u56de\u62a5\u3002", "neutral", 80),
                    new Choice("\u81ea\u7531\u804c\u4e1a", "\u81ea\u7531\u7684\u4ee3\u4ef7\u662f\u4e0d\u7a33\u5b9a\u3002", "neutral", 50, "\u81ea\u7531\u4e4b\u7ffc") }),
                new GridEvent(28, "\u4eba\u751f\u6289\u62e9", "\u662f\u5192\u9669\u521b\u4e1a\u8fd8\u662f\u7a33\u5b9a\u53d1\u5c55\uff1f", 0, false, 0, new[] {
                    new Choice("\u8f9e\u804c\u521b\u4e1a", "\u4f60\u62bc\u4e0a\u4e86\u6240\u6709\u79ef\u84c4\u3002", "neutral", -200),
                    new Choice("\u7ee7\u7eed\u6253\u5de5\uff0c\u7a33\u6b65\u664b\u5347", "\u5b89\u5168\u4f46\u6709\u4e9b\u4e0d\u7518\u5fc3\u3002", "neutral", 200),
                    new Choice("\u51fa\u56fd\u6df1\u9020", "\u544a\u522b\u719f\u6089\u7684\u4e00\u5207\u3002", "selfless", -300, "\u6d77\u5916\u6e38\u5b50") }),
                new GridEvent(30, "\u800c\u7acb\u4e4b\u5e74", "\u4e09\u5341\u800c\u7acb\u3002\u4f60\u7acb\u4f4f\u4e86\u5417\uff1f", 200, false, 0, new[] {
                    new Choice("\u7ed3\u5a5a\u6210\u5bb6", "\u4ece\u6b64\u6709\u4eba\u7b49\u4f60\u56de\u5bb6\u3002", "help", -100, "\u6210\u5bb6\u7acb\u4e1a"),
                    new Choice("\u7ee7\u7eed\u5355\u8eab", "\u4e00\u4e2a\u4eba\u4e5f\u53ef\u4ee5\u6d3b\u5f97\u7cbe\u5f69\u3002", "neutral", 100) }),
                new GridEvent(35, "\u4e2d\u5e74\u5371\u673a", "\u623f\u8d37\u3001\u5b69\u5b50\u3001\u7236\u6bcd\u7684\u5065\u5eb7...", 100, true, 0.03, new[] {
                    new Choice("\u54ac\u7259\u6297\u4f4f", "\u4f60\u53d8\u5f97\u66f4\u52a0\u575a\u5f3a\u3002", "selfless", -50),
                    new Choice("\u501f\u9152\u6d88\u6101", "\u9152\u9192\u4e4b\u540e\uff0c\u95ee\u9898\u8fd8\u5728\u3002", "selfish", -100),
                    new Choice("\u5bfb\u6c42\u5fc3\u7406\u54a8\u8be2", "\u4e13\u4e1a\u7684\u5e2e\u52a9\u8ba9\u4f60\u627e\u5230\u4e86\u65b9\u5411\u3002", "help", -30, "\u76f4\u9762\u5185\u5fc3") }),
                new GridEvent(40, "\u610f\u5916\u4e8b\u6545", "\u4e00\u573a\u7a81\u5982\u5176\u6765\u7684\u53d8\u6545\u3002", 0, true, 0.08, new[] {
                    new Choice("\u79ef\u6781\u6cbb\u7597\uff0c\u4e50\u89c2\u9762\u5bf9", "\u4f60\u66f4\u73cd\u60dc\u751f\u547d\u4e86\u3002", "selfless", -200, "\u6d74\u706b\u91cd\u751f"),
                    new Choice("\u6d88\u6781\u5e94\u5bf9", "\u4f24\u75db\u7559\u4e0b\u4e86\u75d5\u8ff9\u3002", "selfish", -100) }),
                new GridEvent(45, "\u7236\u6bcd\u8001\u53bb", "\u4f60\u7b2c\u4e00\u6b21\u53d1\u73b0\u7236\u6bcd\u7684\u5934\u53d1\u5168\u767d\u4e86\u3002", 0, false, 0, new[] {
                    new Choice("\u653e\u4e0b\u5de5\u4f5c\uff0c\u591a\u966a\u4f34\u7236\u6bcd", "\u4ed6\u4eec\u7b11\u5f97\u50cf\u4e2a\u5b69\u5b50\u3002", "help", -100, "\u5b5d\u5fc3\u53ef\u9274"),
                    new Choice("\u7ed9\u94b1\u8bf7\u4fdd\u59c6\u7167\u987e", "\u5b9e\u9645\u4f46\u603b\u89c9\u5f97\u5c11\u4e86\u4ec0\u4e48\u3002", "neutral", -200),
                    new Choice("\u592a\u5fd9\u4e86\uff0c\u4e0b\u6b21\u518d\u8bf4", "\u53ef\u662f\u6709\u4e9b\u4e8b\u60c5\u6ca1\u6709\u4e0b\u6b21\u3002", "ignore") }),
                new GridEvent(50, "\u77e5\u5929\u547d", "\u4e94\u5341\u77e5\u5929\u547d\u3002\u56de\u671b\u6765\u8def\uff0c\u611f\u6168\u4e07\u5343\u3002", 300, true, 0.05),
                new GridEvent(55, "\u9000\u4f11\u5012\u8ba1\u65f6", "\u5f00\u59cb\u89c4\u5212\u9000\u4f11\u540e\u7684\u751f\u6d3b\u3002", 200, true, 0.06, new[] {
                    new Choice("\u5b66\u4e60\u65b0\u6280\u80fd\uff0c\u51c6\u5907\u7b2c\u4e8c\u4eba\u751f", "\u4f60\u62a5\u4e86\u4e66\u6cd5\u73ed\u548c\u6444\u5f71\u8bfe\u3002", "selfless", -50, "\u7ec8\u8eab\u5b66\u4e60"),
                    new Choice("\u5b89\u5b89\u9759\u9759\u7b49\u9000\u4f11", "\u5e73\u6de1\u4e5f\u662f\u4e00\u79cd\u5e78\u798f\u3002", "neutral") }),
                new GridEvent(60, "\u9000\u4f11", "\u7ec8\u4e8e\u53ef\u4ee5\u6b47\u4e00\u6b47\u4e86\u3002", 300, true, 0.08),
                new GridEvent(65, "\u65c5\u884c", "\u8d81\u8fd8\u8d70\u5f97\u52a8\uff0c\u53bb\u770b\u770b\u8fd9\u4e2a\u4e16\u754c\u3002", 0, true, 0.1, new[] {
                    new Choice("\u73af\u6e38\u4e16\u754c", "\u4f60\u770b\u5230\u4e86\u6781\u5149\u3001\u6c99\u6f20\u3001\u548c\u65e0\u5c3d\u7684\u5927\u6d77\u3002", "selfless", -500, "\u4e16\u754c\u65c5\u4eba"),
                    new Choice("\u56de\u8001\u5bb6\u770b\u770b", "\u90a3\u68f5\u8001\u69d0\u6811\u8fd8\u5728\u3002", "help", -50) }),
                new GridEvent(70, "\u542b\u9970\u5f04\u5b59", "\u770b\u7740\u5b59\u8f88\u957f\u5927\uff0c\u662f\u6700\u5927\u7684\u5e78\u798f\u3002", 100, true, 0.15),
                new GridEvent(80, "\u75c5\u75db", "\u8eab\u4f53\u8d8a\u6765\u8d8a\u529b\u4e0d\u4ece\u5fc3\u3002", 50, true, 0.25, new[] {
                    new Choice("\u5766\u7136\u9762\u5bf9", "\u4f60\u5199\u597d\u4e86\u9057\u5631\uff0c\u5b89\u6392\u597d\u4e86\u4e00\u5207\u3002", "selfless", 0, "\u4ece\u5bb9\u4e0d\u8feb"),
                    new Choice("\u4e0d\u670d\u8001\uff0c\u7ee7\u7eed\u6298\u817e", "\u7cbe\u795e\u53ef\u5609\uff0c\u4f46\u8eab\u4f53\u6297\u8bae\u4e86\u3002", "neutral", -100) }),
                new GridEvent(85, "\u56de\u5fc6\u5f55", "\u4f60\u5f00\u59cb\u5199\u56de\u5fc6\u5f55\u3002", 0, true, 0.3, new[] {
                    new Choice("\u628a\u6545\u4e8b\u8bb2\u7ed9\u5b59\u8f88\u542c", "\u4ed6\u4eec\u542c\u5f97\u5165\u4e86\u8ff7\u3002", "help", 0, "\u4f20\u627f\u8005"),
                    new Choice("\u9501\u8fdb\u62bd\u5c49", "\u6709\u4e9b\u6545\u4e8b\u53ea\u5c5e\u4e8e\u81ea\u5df1\u3002", "neutral") }),
                new GridEvent(90, "\u5915\u9633", "\u5750\u5728\u6447\u6905\u4e0a\uff0c\u770b\u6700\u540e\u4e00\u62b9\u5915\u9633\u3002", 0, true, 0.4),
                new GridEvent(95, "\u6700\u540e\u7684\u68a6", "\u4f60\u68a6\u5230\u4e86\u5c0f\u65f6\u5019\u7684\u90a3\u6761\u6cb3\u3002", 0, true, 0.5),
                new GridEvent(100, "\u767e\u5c81\u4eba\u745e", "\u4f60\u89c1\u8bc1\u4e86\u4e00\u4e2a\u4e16\u7eaa\u7684\u53d8\u8fc1\u3002", 1000, true, 0.5, new[] {
                    new Choice("\u5fae\u7b11\u7740\u95ed\u4e0a\u773c\u775b", "\u8fd9\u4e00\u751f\uff0c\u503c\u4e86\u3002", "selfless", 0, "\u5706\u6ee1\u4eba\u751f"),
                    new Choice("\u8fd8\u60f3\u518d\u770b\u4e00\u6b21\u65e5\u51fa", "\u660e\u5929\u7684\u592a\u9633\uff0c\u4e3a\u4f60\u800c\u5347\u3002", "neutral", 0, "\u4e0d\u820d\u663c\u591c") })
            };
        }
    }
}