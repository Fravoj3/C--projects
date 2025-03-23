using System;
using System.Collections.Generic;
using System.Text;
namespace simulace
{
    public enum TypUdalosti 
    {
        Start,
        Trpelivost,
        Obslouzen
    }

    public class Udalost
    {
        public int kdy;
        public Proces kdo;
        public TypUdalosti co;
        public Udalost(int kdy, Proces kdo, TypUdalosti co)
        {
            this.kdy = kdy;
            this.kdo = kdo;
            this.co = co;

        }
    }
    public class Kalendar
    {
        private List<Udalost> seznam;
        public Kalendar()
        {
            seznam = new List<Udalost>();
        }
        public void Pridej(int kdy, Proces kdo, TypUdalosti co)
        {
            //Console.WriteLine("PLAN: {0} {1} {2}", kdy, kdo.ID, co);

            seznam.Add(new Udalost(kdy, kdo, co));
        }
        public void Odeber(Proces kdo, TypUdalosti co)
        {
            foreach (Udalost ud in seznam)
            {
                if ((ud.kdo == kdo) && (ud.co == co))
                {
                    seznam.Remove(ud);
                    return; // odebiram jen jeden vyskyt!
                }
            }
        }
        public Udalost Prvni()
        {
            Udalost prvni = null;
            foreach (Udalost ud in seznam)
                if ((prvni == null) || (ud.kdy < prvni.kdy))
                    prvni = ud;
            seznam.Remove(prvni);
            return prvni;
        }
        public Udalost Vyber()
        {
            return Prvni();
        }

    }

    public abstract class Proces
    {
        public static char[] mezery = { ' ' };
        public int patro;
        public string ID;
        public abstract void Zpracuj(Udalost ud);
        public void log(string zprava)
        {
            //if (ID == "Dana")
            //if (ID == "elefant")
            //if (this is Zakaznik)
            //    Console.WriteLine($"{model.Cas}/{patro} {ID}: {zprava}");
        }
        protected Model model;
    }

    public class Cas{
        private ModelStatistikaTriDruhu model;
        public Cas(ModelStatistikaTriDruhu model){
            this.model = model;
        }
        public int obsluhovan = 0;
        public int veFronteObchodu = 0;
        public int veFronteVytahu = 0;
        public int posledniCasKonce = 0;
        public enum Stav{
            CekaNaVytah,
            CekaNaObsluhu,
            Obsluhovany, 
            Nevesel
        }
        public Stav stav = Stav.Nevesel;
        public void skonciAkci(){
            if (stav == Stav.Obsluhovany){
                int delka = model.Cas - posledniCasKonce;
                obsluhovan += delka;
            }
            if(stav == Stav.CekaNaVytah){
                int delka = model.Cas - posledniCasKonce;
                veFronteVytahu += delka;
            }
            if(stav == Stav.CekaNaObsluhu){
                int delka = model.Cas - posledniCasKonce;
                veFronteObchodu += delka;
            }
            posledniCasKonce = model.Cas;
        }
        public int celkovyCas(){
            return obsluhovan + veFronteObchodu + veFronteVytahu;
        }
        public void prictiCas(Cas cas){
            obsluhovan += cas.obsluhovan;
            veFronteObchodu += cas.veFronteObchodu;
            veFronteVytahu += cas.veFronteVytahu;
        }
        public void spocitejPrumer(int pocetZakazniku){
            if (pocetZakazniku == 0)
                return;
            obsluhovan = obsluhovan / pocetZakazniku;
            veFronteObchodu = veFronteObchodu / pocetZakazniku;
            veFronteVytahu = veFronteVytahu / pocetZakazniku;
        }
        public void odectiCas(Cas kOdecteni){
            obsluhovan -= kOdecteni.obsluhovan;
            veFronteObchodu -= kOdecteni.veFronteObchodu;
            veFronteVytahu -= kOdecteni.veFronteVytahu;
        }
    }
    public class Oddeleni : Proces
    {        
        public int rychlost;
        public List<Zakaznik> fronta;
        public bool obsluhuje;

        public Oddeleni(Model model, string popis)
        {
            this.model = model;
            string[] popisy = popis.Split(Proces.mezery, StringSplitOptions.RemoveEmptyEntries);
            this.ID = popisy[0];
            this.patro = int.Parse(popisy[1]);
            if (this.patro > model.MaxPatro)
                model.MaxPatro = this.patro;
            this.rychlost = int.Parse(popisy[2]);
            obsluhuje = false;
            fronta = new List<Zakaznik>();
            model.VsechnaOddeleni.Add(this);
        }
        public void ZaradDoFronty(Zakaznik zak)
        {
            fronta.Add(zak);
            log("do fronty " + zak.ID);

            if (obsluhuje){
                if (zak is ZakaznikTriDruhu zakTriDruhu)
                {
                    zakTriDruhu.cas.stav = Cas.Stav.CekaNaObsluhu;
                }
            } // nic
            else
            {
                if (zak is ZakaznikTriDruhu zakTriDruhu)
                {
                    zakTriDruhu.cas.stav = Cas.Stav.Obsluhovany;
                }
                obsluhuje = true;
                model.Naplanuj(model.Cas, this, TypUdalosti.Start);
            }
        }
        public void VyradZFronty(Zakaznik koho)
        {
            fronta.Remove(koho);
        }
        public override void Zpracuj(Udalost ud)
        {
            switch (ud.co)
            {
                case TypUdalosti.Start:
                    if (fronta.Count == 0)
                        obsluhuje = false; // a dal neni naplanovana a probudi se tim, ze se nekdo zaradi do fronty
                    else
                    {
                        Zakaznik zak = fronta[0];
                        fronta.RemoveAt(0);
                        model.Odplanuj(zak, TypUdalosti.Trpelivost);
                        model.Naplanuj(model.Cas + rychlost, zak, TypUdalosti.Obslouzen);
                        if (zak is ZakaznikTriDruhu zakTriDruhu)
                        {
                            zakTriDruhu.cas.stav = Cas.Stav.Obsluhovany;
                        }
                        model.Naplanuj(model.Cas + rychlost, this, TypUdalosti.Start);
                    }
                    break;
            }
        }
    }
    public enum SmeryJizdy
    {
        Nahoru,
        Dolu,
        Stoji
    }
    public class Vytah : Proces
    {
        private int kapacita;
        private int dobaNastupu;
        private int dobaVystupu;
        private int dobaPatro2Patro;
        static int[] ismery = { +1, -1, 0 }; // prevod (int) SmeryJizdy na smer

        private class Pasazer
        {
            public Proces kdo;
            public int kamJede;
            public Pasazer(Proces kdo, int kamJede)
            {
                this.kdo = kdo;
                this.kamJede = kamJede;
            }
        }

        private List<Pasazer>[,] cekatele; // [patro,smer]
        private List<Pasazer> naklad;   // pasazeri ve vytahu
        private SmeryJizdy smer;
        private int kdyJsemMenilSmer;

        public int jakAsiDlouhoPojeduVytahem(int odkud, int kam)
        {
            int lidiVeFronte = 0;
            for (int i = 0; i < 2; ++i){
                lidiVeFronte += cekatele[odkud, i].Count;
            }
            int dobaObsluhyJedneSkupinyMax = (dobaNastupu + dobaVystupu + dobaPatro2Patro) * model.MaxPatro  * 2;
            int asiOcekavanaDobaObsluhySkupiny = (int) ((double)dobaObsluhyJedneSkupinyMax * 1.7);
            int pocetSkupinPredSebou = lidiVeFronte / kapacita;
            int casNezSeDostanuNaRadu = pocetSkupinPredSebou * asiOcekavanaDobaObsluhySkupiny;

            int casMeJizdyAsi = dobaNastupu + dobaVystupu + dobaPatro2Patro * Math.Abs(odkud - kam);
            return casNezSeDostanuNaRadu + casMeJizdyAsi;
        }

        public void PridejDoFronty(int odkud, int kam, Proces kdo)
        {
            Pasazer pas = new Pasazer(kdo, kam);
            if (kam > odkud)
                cekatele[odkud, (int)SmeryJizdy.Nahoru].Add(pas);
            else
                cekatele[odkud, (int)SmeryJizdy.Dolu].Add(pas);

            // pripadne rozjet stojici vytah:
            if (smer == SmeryJizdy.Stoji)
            {
                model.Odplanuj(model.vytah, TypUdalosti.Start); // kdyby nahodou uz byl naplanovany
                model.Naplanuj(model.Cas, this, TypUdalosti.Start);
            }
        }
        public bool CekaNekdoVPatrechVeSmeruJizdy()
        {
            int ismer = ismery[(int)smer];
            for (int pat = patro + ismer; (pat > 0) && (pat <= model.MaxPatro); pat += ismer)
                if ((cekatele[pat, (int)SmeryJizdy.Nahoru].Count > 0) || (cekatele[pat, (int)SmeryJizdy.Dolu].Count > 0))
                {
                    if (cekatele[pat, (int)SmeryJizdy.Nahoru].Count > 0)
                        log("Nahoru čeká " + cekatele[pat, (int)SmeryJizdy.Nahoru][0].kdo.ID
                            + " v patře " + pat + "/" + cekatele[pat, (int)SmeryJizdy.Nahoru][0].kdo.patro);
                    if (cekatele[pat, (int)SmeryJizdy.Dolu].Count > 0)
                        log("Dolů čeká " + cekatele[pat, (int)SmeryJizdy.Dolu][0].kdo.ID
                            + " v patře " + pat + "/" + cekatele[pat, (int)SmeryJizdy.Dolu][0].kdo.patro);

                    //log(" x "+cekatele[pat, (int)SmeryJizdy.Nahoru].Count+" x "+cekatele[pat, (int)SmeryJizdy.Dolu].Count);
                    return true;
                }
            return false;
        }

        public Vytah(Model model, string popis)
        {
            this.model = model;
            string[] popisy = popis.Split(Proces.mezery, StringSplitOptions.RemoveEmptyEntries);
            this.ID = popisy[0];
            this.kapacita = int.Parse(popisy[1]);
            this.dobaNastupu = int.Parse(popisy[2]);
            this.dobaVystupu = int.Parse(popisy[3]);
            this.dobaPatro2Patro = int.Parse(popisy[4]);
            this.patro = 0;
            this.smer = SmeryJizdy.Stoji;
            this.kdyJsemMenilSmer = -1;

            cekatele = new List<Pasazer>[model.MaxPatro + 1, 2];
            for (int i = 0; i < model.MaxPatro + 1; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    cekatele[i, j] = new List<Pasazer>();
                }

            }
            naklad = new List<Pasazer>();
        }
        public override void Zpracuj(Udalost ud)
        {
            switch (ud.co)
            {
                case TypUdalosti.Start:

                    // HACK pro cerstve probuzeny vytah:
                    if (smer == SmeryJizdy.Stoji)
                        // stoji, tedy nikoho neveze a nekdo ho prave probudil => nastavim jakykoliv smer a najde ho:
                        smer = SmeryJizdy.Nahoru;

                    // chce nekdo vystoupit?
                    foreach (Pasazer pas in naklad)
                        if (pas.kamJede == patro)
                        // bude vystupovat:
                        {
                            naklad.Remove(pas);

                            pas.kdo.patro = patro;
                            model.Naplanuj(model.Cas + dobaVystupu, pas.kdo, TypUdalosti.Start);
                            log("vystupuje " + pas.kdo.ID);

                            model.Naplanuj(model.Cas + dobaVystupu, this, TypUdalosti.Start);

                            return; // to je pro tuhle chvili vsechno
                        }

                    // muze a chce nekdo nastoupit?
                    if (naklad.Count == kapacita)
                    // i kdyby chtel nekdo nastupovat, nemuze; veze lidi => pokracuje:
                    {
                        // popojet:
                        int ismer = ismery[(int)smer];
                        patro = patro + ismer;

                        string spas = "";
                        foreach (Pasazer pas in naklad)
                            spas += " " + pas.kdo.ID;
                        log("odjíždím");
                        model.Naplanuj(model.Cas + dobaPatro2Patro, this, TypUdalosti.Start);
                        return; // to je pro tuhle chvili vsechno
                    }
                    else
                    // neni uplne plny
                    {
                        // chce nastoupit nekdo VE SMERU jizdy?
                        if (cekatele[patro, (int)smer].Count > 0)
                        {
                            log("nastupuje " + cekatele[patro, (int)smer][0].kdo.ID);
                            naklad.Add(cekatele[patro, (int)smer][0]);
                            cekatele[patro, (int)smer].RemoveAt(0);
                            model.Naplanuj(model.Cas + dobaNastupu, this, TypUdalosti.Start);

                            return; // to je pro tuhle chvili vsechno
                        }

                        // ve smeru jizdy nikdo nenastupuje:
                        if (naklad.Count > 0)
                        // nikdo nenastupuje, vezu pasazery => pokracuju v jizde:
                        {
                            // popojet:
                            int ismer = ismery[(int)smer];
                            patro = patro + ismer;

                            string spas = "";
                            foreach (Pasazer pas in naklad)
                                spas += " " + pas.kdo.ID;
                            //log("nekoho vezu");
                            log("odjíždím: " + spas);

                            model.Naplanuj(model.Cas + dobaPatro2Patro, this, TypUdalosti.Start);
                            return; // to je pro tuhle chvili vsechno
                        }

                        // vytah je prazdny, pokud v dalsich patrech ve smeru jizdy uz nikdo neceka, muze zmenit smer nebo se zastavit:
                        if (CekaNekdoVPatrechVeSmeruJizdy() == true)
                        // pokracuje v jizde:
                        {
                            // popojet:
                            int ismer = ismery[(int)smer];
                            patro = patro + ismer;

                            //log("nekdo ceka");
                            log("odjíždím");
                            model.Naplanuj(model.Cas + dobaPatro2Patro, this, TypUdalosti.Start);
                            return; // to je pro tuhle chvili vsechno
                        }

                        // ve smeru jizdy uz nikdo neceka => zmenit smer nebo zastavit:
                        if (smer == SmeryJizdy.Nahoru)
                            smer = SmeryJizdy.Dolu;
                        else
                            smer = SmeryJizdy.Nahoru;

                        log("změna směru");

                        //chce nekdo nastoupit prave tady?
                        if (kdyJsemMenilSmer != model.Cas)
                        {
                            kdyJsemMenilSmer = model.Cas;
                            // podivat se, jestli nekdo nechce nastoupit opacnym smerem:
                            model.Naplanuj(model.Cas, this, TypUdalosti.Start);
                            return;
                        }

                        // uz jsem jednou smer menil a zase nikdo nenastoupil a nechce => zastavit
                        log("zastavuje");
                        smer = SmeryJizdy.Stoji;
                        return; // to je pro tuhle chvili vsechno
                    }
            }
        }
    }
    public class Zakaznik : Proces
    {
        protected int trpelivost;
        protected int prichod;
        protected List<string> Nakupy;
        int pocetNakupu = 0;
        public Zakaznik(Model model, string popis)
        {
            this.model = model;
            string[] popisy = popis.Split(Proces.mezery, StringSplitOptions.RemoveEmptyEntries);
            this.ID = popisy[0];
            this.prichod = int.Parse(popisy[1]);
            this.trpelivost = int.Parse(popisy[2]);
            Nakupy = new List<string>();
            for (int i = 3; i < popisy.Length; i++)
            {
                Nakupy.Add(popisy[i]);
                pocetNakupu++;
            }
            this.patro = 0;
            //Console.WriteLine("Init Zakaznik: {0}", ID);
            model.Naplanuj(prichod, this, TypUdalosti.Start);
        }
        public override void Zpracuj(Udalost ud)
        {
            switch (ud.co)
            {
                case TypUdalosti.Start:
                    if (Nakupy.Count == 0)
                    // ma nakoupeno
                    {
                        if (patro == 0){
                            log("-------------- odchází"); // nic, konci
                            int stravenyCas = model.Cas - prichod;
                            (model as ModelStatistika)?.prictiStravenyCas(stravenyCas);
                            //Console.WriteLine("Zákazník {0} strávil v obchodě {1} minut a navstivil {2} obchodu", ID, stravenyCas, pocetNakupu);
                        }
                        else
                            model.vytah.PridejDoFronty(patro, 0, this);
                    }
                    else
                    {
                        Oddeleni odd = OddeleniPodleJmena(Nakupy[0]);
                        int pat = odd.patro;
                        if (pat == patro) // to oddeleni je v patre, kde prave jsem
                        {
                            if (Nakupy.Count > 1)
                                model.Naplanuj(model.Cas + trpelivost, this, TypUdalosti.Trpelivost);
                            odd.ZaradDoFronty(this);
                        }
                        else
                            model.vytah.PridejDoFronty(patro, pat, this);
                    }
                    break;
                case TypUdalosti.Obslouzen:
                    log("Nakoupeno: " + Nakupy[0]);
                    Nakupy.RemoveAt(0);
                    // ...a budu hledat dalsi nakup -->> Start
                    model.Naplanuj(model.Cas, this, TypUdalosti.Start);
                    break;
                case TypUdalosti.Trpelivost:
                    log("!!! Trpělivost: " + Nakupy[0]);
                    // vyradit z fronty:
                    {
                        Oddeleni odd = OddeleniPodleJmena(Nakupy[0]);
                        odd.VyradZFronty(this);
                    }

                    // prehodit tenhle nakup na konec:
                    string nesplneny = Nakupy[0];
                    Nakupy.RemoveAt(0);
                    Nakupy.Add(nesplneny);

                    // ...a budu hledat dalsi nakup -->> Start
                    model.Naplanuj(model.Cas, this, TypUdalosti.Start);
                    break;
            }
        }

        protected Oddeleni OddeleniPodleJmena(string kamChci)
        {
            foreach (Oddeleni odd in model.VsechnaOddeleni)
                if (odd.ID == kamChci)
                    return odd;
            return null;
        }
    }

    public class ZakaznikTriDruhu : Zakaznik
    {
        int typ;
        public Cas cas;
        new ModelStatistikaTriDruhu model;

        Oddeleni chciNavstivit;
        public ZakaznikTriDruhu(ModelStatistikaTriDruhu model, string popis, int typZakaznika): base(model, popis)
        {
            this.typ = typZakaznika;
            this.model = model;
            this.chciNavstivit = null;
            this.cas = new Cas(this.model);
        }
        public Oddeleni ziskejOddeleniVPatre(int patro){
            for(int i = 0; i < Nakupy.Count; i++){
                Oddeleni odd = OddeleniPodleJmena(Nakupy[i]);
                if (odd.patro == patro)
                    return odd;
            }
            return null;
        }

        public Oddeleni ziskejNejrychlejsiOddeleni(){
            int nejkratsiCekani = int.MaxValue;
            Oddeleni nejrychlejsi = null;
            for(int i = 0; i < Nakupy.Count; ++i){
                Oddeleni odd = OddeleniPodleJmena(Nakupy[i]);
                int cekaniUOddeleni = odd.fronta.Count * odd.rychlost;
                int casNaVytah = 0;
                if (odd.patro != patro){
                    casNaVytah = model.vytah.jakAsiDlouhoPojeduVytahem(patro, odd.patro);
                }
                int doba = cekaniUOddeleni + casNaVytah;
                if (doba < nejkratsiCekani){
                    nejkratsiCekani = doba;
                    nejrychlejsi = odd;
                }
            }
            return nejrychlejsi;
        }

        public Oddeleni ziskejOddeleniPodleTypuZakaznika(){
            if (typ == 1)
                return OddeleniPodleJmena(Nakupy[0]);
            if (typ == 2){
                Oddeleni kamChci = ziskejOddeleniVPatre(patro);
                if (kamChci == null)
                    kamChci = OddeleniPodleJmena(Nakupy[0]);
                return kamChci;
            }
            if (typ == 0){
                return ziskejNejrychlejsiOddeleni();
            }
            return null;
        }
        
        public void odstranZeSeznamu(Oddeleni odd){
            if (odd == null)
                Console.WriteLine("ziskal null jako oddeleni k odstraneni");
            for (int i = 0; i < Nakupy.Count; i++){
                if (Nakupy[i] == odd.ID){
                    Nakupy.RemoveAt(i);
                    return;
                }
            }
        }
        public override void Zpracuj(Udalost ud)
        {
            switch (ud.co)
            {
                case TypUdalosti.Start:
                    cas.skonciAkci();
                    if (Nakupy.Count == 0)
                    // ma nakoupeno
                    {
                        if (patro == 0){
                            log("-------------- odchází"); // nic, konci
                            int stravenyCas = model.Cas - prichod;
                            model.prictiStravenyCas(stravenyCas, typ, this.cas);
                            //Console.WriteLine("Zákazník {0} strávil v obchodě {1} minut a navstivil {2} obchodu", ID, stravenyCas, pocetNakupu);
                        }
                        else{ 
                            model.vytah.PridejDoFronty(patro, 0, this);
                            cas.stav = Cas.Stav.CekaNaVytah;
                        }
                    }
                    else
                    {
                        if (cas.stav == Cas.Stav.Nevesel){
                            cas.posledniCasKonce = model.Cas;
                        }
                        Oddeleni odd = ziskejOddeleniPodleTypuZakaznika();
                        chciNavstivit = odd;
                        int pat = odd.patro;
                        if (pat == patro) // to oddeleni je v patre, kde prave jsem
                        {
                            if (Nakupy.Count > 1)
                                model.Naplanuj(model.Cas + trpelivost, this, TypUdalosti.Trpelivost);
                            odd.ZaradDoFronty(this);
                        }
                        else{ 
                            cas.stav = Cas.Stav.CekaNaVytah;
                            model.vytah.PridejDoFronty(patro, pat, this);
                        }
                    }
                    break;
                case TypUdalosti.Obslouzen:
                    odstranZeSeznamu(chciNavstivit);
                    // ...a budu hledat dalsi nakup -->> Start
                    model.Naplanuj(model.Cas, this, TypUdalosti.Start);
                    break;
                case TypUdalosti.Trpelivost:
                    // vyradit z fronty:
                    {
                        chciNavstivit.VyradZFronty(this);
                    }

                    // prehodit tenhle nakup na konec:
                    odstranZeSeznamu(chciNavstivit);
                    Nakupy.Add(chciNavstivit.ID);

                    // ...a budu hledat dalsi nakup -->> Start
                    model.Naplanuj(model.Cas, this, TypUdalosti.Start);
                    break;
            }
        }
    }


    public class Model
    {
        public int Cas;
        public Vytah vytah;
        public List<Oddeleni> VsechnaOddeleni = new List<Oddeleni>();
        public int MaxPatro;
        protected Kalendar kalendar;
        public void Naplanuj(int kdy, Proces kdo, TypUdalosti co)
        {
            kalendar.Pridej(kdy, kdo, co);
        }
        public void Odplanuj(Proces kdo, TypUdalosti co)
        {
            kalendar.Odeber(kdo, co);
        }
        public virtual void VytvorProcesy()
        {
            System.IO.StreamReader soubor
                = new
          System.IO.StreamReader("obchod_data.txt");
            while (!soubor.EndOfStream)
            {
                string s = soubor.ReadLine();
                if (s != "")
                {
                    switch (s[0])
                    {
                        case 'O':
                            new Oddeleni(this, s.Substring(1));
                            break;
                        case 'Z':
                            new Zakaznik(this, s.Substring(1));
                            break;
                        case 'V':
                            vytah = new Vytah(this, s.Substring(1));
                            break;
                    }
                }
            }
            soubor.Close();
        }
        public int Vypocet()
        {
            Cas = 0;
            kalendar = new Kalendar();
            VytvorProcesy();

            Udalost ud;

            while ((ud = kalendar.Vyber()) != null)
            {
                //Console.WriteLine("{0} {1} {2}", ud.kdy, ud.kdo.ID, ud.co);
                Cas = ud.kdy;
                ud.kdo.Zpracuj(ud);
            }
            return Cas;
        }
    }


    public class ModelStatistika:Model
    {
        protected Random random = new Random(12345);
        protected bool nactenaKonfiguraceObchodu = false;

        public int soucetDobyNavstevniku = 0;

        public void prictiStravenyCas(int cas){
            this.soucetDobyNavstevniku += cas;
        }
        public void nactiKonfiguraciObchodu(){
            // Nacteme sezam oddeleni a konfiguraci vytahu ze souboru
            System.IO.StreamReader soubor = new System.IO.StreamReader("C:\\Users\\fravo\\Documents\\C# projects\\SimulaceObchod\\obchod_data.txt");
            while (!soubor.EndOfStream)
            {
                string s = soubor.ReadLine();
                if (s != "")
                {
                    switch (s[0])
                    {
                        case 'O':
                            new Oddeleni(this, s.Substring(1));
                            break;
                        case 'V':
                            vytah = new Vytah(this, s.Substring(1));
                            break;
                    }
                }
            }
            soubor.Close();
            nactenaKonfiguraceObchodu = true;
        }
        public virtual void VytvorProcesy(int pocetZakazniku)
        {
            // Nacti konfiguraci obchodu
            if (!nactenaKonfiguraceObchodu)
                nactiKonfiguraciObchodu();
            // Vygenerovani zakazniku
            for (int i = 0; i < pocetZakazniku; i++)
            {
                int CasPrichodu = random.Next(0, 601);
                int trpevilost = random.Next(1, 181);
                int pocetNakupu = random.Next(1, 21);
                string oddeleniKNavsteve = "";
                int pocetOddeleni = base.VsechnaOddeleni.Count;
                for (int j = 0; j < pocetNakupu; j++)
                {
                    int idOddeleni = random.Next(0, pocetOddeleni);
                    if (j != 0){
                        oddeleniKNavsteve += " ";
                    }
                    oddeleniKNavsteve += base.VsechnaOddeleni[idOddeleni].ID;
                }
                new Zakaznik(this, i+" "+CasPrichodu+" "+trpevilost+" "+oddeleniKNavsteve);
            }

        }
        public int Vypocet(int pocetZakazniku)
        {
            Cas = 0;
            soucetDobyNavstevniku = 0;
            kalendar = new Kalendar();
            VytvorProcesy(pocetZakazniku);

            Udalost ud;

            while ((ud = kalendar.Vyber()) != null)
            {
                Cas = ud.kdy;
                ud.kdo.Zpracuj(ud);
            }
            return soucetDobyNavstevniku/pocetZakazniku;
        }
    
        public int PrumerProDanyPocetZakazniku(int pocetZakazniku){
            int minimum = int.MaxValue;
            int maximum = int.MinValue;
            int soucet = 0;
            for (int i = 0; i < 10; i++){
                int cas = Vypocet(pocetZakazniku);
                soucet += cas;
                if (cas < minimum)
                    minimum = cas;
                if (cas > maximum)
                    maximum = cas;
            }
            soucet = soucet - minimum - maximum;
            int prumer = soucet / 8;
            return prumer;
        }
    
        public int[] VypocitejPrumerProRozsahZakazniku(int minimálníZkoumanýPočet, int maximálníZkoumanýPočet){
            bool log = true;
            int procento = 0;
            int[] prumery = new int[maximálníZkoumanýPočet - minimálníZkoumanýPočet + 1];
            for (int i = minimálníZkoumanýPočet; i <= maximálníZkoumanýPočet; i++){
                prumery[i - minimálníZkoumanýPočet] = PrumerProDanyPocetZakazniku(i);
                int procentoNove = (int)((double)(i - minimálníZkoumanýPočet) / (maximálníZkoumanýPočet - minimálníZkoumanýPočet) * 100);
            }
            return prumery;
        }
    }

    public class ModelStatistikaTriDruhu:ModelStatistika
    {
        public void InitializeModelStatistikaTriDruhu(){
            resetCas();
        }
        public void resetCas(){
            for (int i = 0; i < 3; i++){
                soucetDobyNavstevyTypu = getEmptyCas();
            }
        }
        public Cas[] getEmptyCas(){
            Cas[] casy = new Cas[3];
            for (int i = 0; i < 3; i++){
                casy[i] = new Cas(this);
            }
            return casy;
        }
        Cas[] soucetDobyNavstevyTypu = new simulace.Cas[3];
        int[] pocetZakaznikuTypu = {0, 0, 0};
        public void prictiStravenyCas(int cas, int typ){
            //this.soucetDobyNavstevyTypu[typ] += cas;
            this.pocetZakaznikuTypu[typ]++;
        }
        public void prictiStravenyCas(int cas, int typ, Cas casObj){
            this.soucetDobyNavstevyTypu[typ].prictiCas(casObj);
            this.pocetZakaznikuTypu[typ]++;
        }
        public override void VytvorProcesy(int pocetZakazniku)
        {
            // Nacti konfiguraci obchodu
            if (!nactenaKonfiguraceObchodu)
                base.nactiKonfiguraciObchodu();
            // Vygenerovani zakazniku
            for (int i = 0; i < pocetZakazniku; i++)
            {
                int CasPrichodu = random.Next(0, 601);
                int trpevilost = random.Next(1, 181);
                int pocetNakupu = random.Next(1, 21);
                int typZakaznika = (i+1)%3;
                string oddeleniKNavsteve = "";
                int pocetOddeleni = base.VsechnaOddeleni.Count;
                for (int j = 0; j < pocetNakupu; j++)
                {
                    int idOddeleni = random.Next(0, pocetOddeleni);
                    if (j != 0){
                        oddeleniKNavsteve += " ";
                    }
                    oddeleniKNavsteve += base.VsechnaOddeleni[idOddeleni].ID;
                }
                new ZakaznikTriDruhu(this, i+" "+CasPrichodu+" "+trpevilost+" "+oddeleniKNavsteve, typZakaznika);
            }

        }
        public new Cas[] Vypocet(int pocetZakazniku)
        {
            Cas = 0;
            soucetDobyNavstevniku = 0;
            resetCas();
            pocetZakaznikuTypu = new int[] {0, 0, 0};
            kalendar = new Kalendar();
            VytvorProcesy(pocetZakazniku);

            Udalost ud;

            while ((ud = kalendar.Vyber()) != null)
            {
                Cas = ud.kdy;
                ud.kdo.Zpracuj(ud);
            }

            soucetDobyNavstevyTypu[0].spocitejPrumer(pocetZakaznikuTypu[0]);
            soucetDobyNavstevyTypu[1].spocitejPrumer(pocetZakaznikuTypu[1]);
            soucetDobyNavstevyTypu[2].spocitejPrumer(pocetZakaznikuTypu[2]);
            
            return soucetDobyNavstevyTypu;
        }
    
        public new Cas[] PrumerProDanyPocetZakazniku(int pocetZakazniku){
            int[] minimum = {int.MaxValue, int.MaxValue, int.MaxValue};
            Cas[] minimumC = getEmptyCas();
            int[] maximum = {int.MinValue, int.MinValue, int.MinValue};
            Cas[] maximumC = getEmptyCas();
            Cas[] soucty = getEmptyCas();
            for (int i = 0; i < 10; i++){
                Cas[] prumery = Vypocet(pocetZakazniku);
                for (int j = 0; j < 3; j++){
                    soucty[j].prictiCas(prumery[j]);
                }
                for (int j = 0; j < 3; j++){
                    if (prumery[j].celkovyCas() < minimum[j]){
                        minimum[j] = prumery[j].celkovyCas();
                        minimumC[j] = prumery[j];
                    }
                    if (prumery[j].celkovyCas() > maximum[j]){
                        maximum[j] = prumery[j].celkovyCas();
                        maximumC[j] = prumery[j];
                    }
                }
            }
            for (int j = 0; j < 3; j++){
                soucty[j].odectiCas(minimumC[j]);
                soucty[j].odectiCas(maximumC[j]);
                soucty[j].spocitejPrumer(8);
            }
            return soucty;
        }
    
        public new Cas[][] VypocitejPrumerProRozsahZakazniku(int minimálníZkoumanýPočet, int maximálníZkoumanýPočet){
            bool log = true;
            int procento = 0;
            Cas[][] prumery = new Cas[maximálníZkoumanýPočet - minimálníZkoumanýPočet + 1][];
            for (int i = 0; i <= maximálníZkoumanýPočet - minimálníZkoumanýPočet; i++){
                prumery[i] = PrumerProDanyPocetZakazniku(i+1);
                int procentoNove = (int)((double)(i) / (maximálníZkoumanýPočet - minimálníZkoumanýPočet) * 100);
                if (procentoNove > procento){
                    procento = procentoNove;
                    if (log)
                        Console.Write("\r{0}%   ", procento);
                }
            }
            Console.WriteLine();
            return prumery;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ModelStatistikaTriDruhu model = new ModelStatistikaTriDruhu();
            int dolniPocetZakazniku = 1;
            Cas[][] vysledek = model.VypocitejPrumerProRozsahZakazniku(dolniPocetZakazniku, 501);
            //Console.ReadLine();
            for (int i = 0; i < vysledek.Length; i++){
                Cas druh1 = vysledek[i][1];
                Cas druh2 = vysledek[i][2];
                Cas druh3 = vysledek[i][0];
                Console.WriteLine($"{dolniPocetZakazniku + i}\t{druh1.celkovyCas()}\t{druh1.obsluhovan}\t{druh1.veFronteObchodu}\t{druh1.veFronteVytahu}\t{druh2.celkovyCas()}\t{druh2.obsluhovan}\t{druh2.veFronteObchodu}\t{druh2.veFronteVytahu}\t{druh3.celkovyCas()}\t{druh3.obsluhovan}\t{druh3.veFronteObchodu}\t{druh3.veFronteVytahu}");
            }
        }
    }
}