using System;
using System.Collections.Generic;
using System.Linq;

namespace OyunTeorisi
{
    // Karakter Stratejilerini temsil eden temsilci (delegate)
    public delegate bool StrategyFunc(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered);

    // Turnuva boyunca bir karakter tipinin durumunu tutar
    public class KarakterDurumu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; } = 0; // Popülasyondaki bu tipten kaç tane var
        public long TotalScore { get; set; } = 0; // Popülasyondaki bu tipin toplam skoru
        public StrategyFunc Strategy { get; set; }
    }

    public class Program
    {
        // Global ve özel (private) değişkenler
        private static Random _rnd = new Random();

        // Hain Hafıza stratejisi için durum takibi (Her maçta sıfırlanacak)
        private static bool _grimTriggerA = false;
        private static bool _grimTriggerB = false;

        // Popülasyon ve Turnuva Ayarları
        private static List<KarakterDurumu> _population = new List<KarakterDurumu>();
        private const int TUR_SAYISI = 100; // Her bir ikili maçtaki tur sayısı (Evrimsel mod için)

        // Karakter adlarını kolayca bulmak için
        private static readonly string[] CharacterNames = {
            "Kopyacı", "Ponçik", "Sinsi", "Hain Hafıza", "Şanslı Cimbom",
            "Affetmez Ayna", "Sömürücü", "Fırsatçı", "Grupçu", "İntikamcı"
        };

        public static void Main()
        {
            Console.Clear();
            Console.Title = "Oyun Teorisi Simülasyonu";

            OyunAmaciniYazdir();
            Console.WriteLine("\n" + new string('=', 80) + "\n");
            PuanTablosunuGoster();
            Console.WriteLine("\n" + new string('=', 80) + "\n");
            KarakterAciklamalariniGoster();

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();

            StartMenu(); // Yeni menü ile başlat
        }

        // Simülasyon Türü Seçim Menüsü
        public static void StartMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("--- LÜTFEN SİMÜLASYON TÜRÜNÜ SEÇİN ---");
            Console.ResetColor();
            Console.WriteLine("1. Tekli Maç (1v1 Detaylı Karşılaştırma)");
            Console.WriteLine("2. Evrimsel Turnuva (Popülasyon Evrimi)");
            Console.WriteLine("0. Çıkış");

            Console.Write("\nSeçiminiz (1/2/0): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    CharSelect_1v1();
                    break;
                case "2":
                    RunSimulation();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Geçersiz seçim. Tekrar deneyin.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                    StartMenu();
                    break;
            }
        }

        #region EKRAN VE BILGI METOTLARI
        // ... (OyunAmaciniYazdir, PuanTablosunuGoster, KarakterAciklamalariniGoster metotları değişmedi)
        public static void OyunAmaciniYazdir()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- OYUN TEORİSİ SİMÜLASYONU: TEKRARLI MAHKUM İKİLEMİ ---\n");
            Console.ResetColor();

            Console.WriteLine("Oyunun Amacı:");
            Console.WriteLine("\tBu simülasyon, karakter popülasyonunun jenerasyonlar içinde nasıl değiştiğini inceler.");
            Console.WriteLine("\tHer Jenerasyonda, tüm karakter tipleri birbirleriyle {0} tur maç yapar.", TUR_SAYISI);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n\tEVRİM KURALI: Her jenerasyon sonunda en düşük puanlı karakterler, en başarılı (yüksek puanlı) karakterlerin kopyalarıyla değiştirilir.");
            Console.ResetColor();
        }

        public static void PuanTablosunuGoster()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- KAZANÇ MATRİSİ (PUAN TABLOSU) ---\n");
            Console.ResetColor();

            Console.WriteLine("\t\t\t| Oyuncu B: İşbirliği (True)\t| Oyuncu B: İhanet (False)");
            Console.WriteLine(new string('-', 100));

            Console.Write("Oyuncu A: İşbirliği (True)");
            Console.Write("\t|");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" Orta Kazanç: A: +2, B: +2 ");
            Console.ResetColor();
            Console.Write("\t|");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" Sömürü: A: -1, B: +3");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine(new string('-', 100));

            Console.Write("Oyuncu A: İhanet (False)");
            Console.Write("\t|");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" Sömürülme: A: +3, B: -1");
            Console.ResetColor();
            Console.Write("\t|");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" Karşılıklı Kayıp: A: 0, B: 0");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine(new string('-', 100));
        }

        public static void KarakterAciklamalariniGoster()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- KARAKTER LİSTESİ VE STRATEJİLERİ ---\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("NO\tKARAKTER ADI\t\tSTRATEJİ ÖZETİ");
            Console.WriteLine(new string('-', 60));
            Console.ResetColor();

            // Karakterler ve açıklamalar (daha kısa ve öz)
            Console.WriteLine("1.\tKopyacı\t\t\tÖnce True, sonra rakibin bir önceki hamlesini kopyalar (Tit-for-Tat).");
            Console.WriteLine("2.\tPonçik\t\t\tRakip ne yaparsa yapsın **HEP TRUE**.");
            Console.WriteLine("3.\tSinsi\t\t\tHer durumda sadece kendi çıkarını düşünür ve **HEP FALSE**.");
            Console.WriteLine("4.\tHain Hafıza\t\tBaşlangıç True. Rakip bir kere False yaparsa, oyun sonuna kadar hep False.");
            Console.WriteLine("5.\tŞanslı Cimbom\t\tHer turda rastgele (random True/False) karar verir.");
            Console.WriteLine("6.\tAffetmez Ayna\t\tKopyacı gibidir, ancak nadiren (%20 ihtimal) rakibin False hamlesini affeder.");
            Console.WriteLine("7.\tSömürücü\t\tBaşlangıç False. Rakip nazikse False devam eder, misilleme gelirse Kopyacı'ya döner.");
            Console.WriteLine("8.\tFırsatçı\t\tKopyacı stratejisi uygular ama arada bir (düşük ihtimalle) sürpriz False yapar.");
            Console.WriteLine("9.\tGrupçu\t\t\tBaşlangıç True. Sonraki hamleleri, geçmiş tüm hamlelerin **çoğunluğuna** göre belirler.");
            Console.WriteLine("10.\tİntikamcı\t\tKazandıysa aynı eylemi tekrarlar, kaybettiyse değiştirir (Pavlov).");
        }

        public static int ReadCharacterSelection(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                if (int.TryParse(input, out int selection) && selection >= 1 && selection <= 10)
                {
                    return selection;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Hatalı seçim. Lütfen 1 ile 10 arasında bir sayı girin.");
                    Console.ResetColor();
                }
            }
        }

        private static string FormatDelta(int delta)
        {
            if (delta > 0) return $"+{delta}";
            return $"{delta}";
        }

        #endregion

        #region 1V1 TEKLİ MAÇ METOTLARI

        public static void CharSelect_1v1()
        {
            Console.Clear();
            KarakterAciklamalariniGoster();
            Console.WriteLine("\n" + new string('-', 60));

            int select1 = ReadCharacterSelection("Karakter A'yı seçin (1-10): ");
            int select2 = ReadCharacterSelection("Karakter B'yi seçin (1-10): ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nSeçimleriniz: A: {CharacterNames[select1 - 1]} vs B: {CharacterNames[select2 - 1]}");
            Console.ResetColor();

            Game_1v1(select1, select2);
        }

        public static void Game_1v1(int select1, int select2)
        {
            Console.WriteLine("\nKaç tur oynanacağını seçin (Örn: 100): ");
            string oyunSayisiStr = Console.ReadLine();

            if (!int.TryParse(oyunSayisiStr, out int sayisalDeger) || sayisalDeger <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Hatalı giriş. Lütfen 0'dan büyük bir sayı girin.");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1500);
                Game_1v1(select1, select2);
                return;
            }

            // Skorları ve durumları sıfırla
            int scoreA = 0;
            int scoreB = 0;
            _grimTriggerA = false;
            _grimTriggerB = false;

            // Dizileri tanımla ve başlat
            bool[] strategyA = new bool[sayisalDeger];
            bool[] strategyB = new bool[sayisalDeger];

            Vs_1v1(select1, select2, strategyA, strategyB, ref scoreA, ref scoreB);
        }

        public static void Vs_1v1(int select1, int select2, bool[] strategyA, bool[] strategyB, ref int scoreA, ref int scoreB)
        {
            StrategyFunc strategyFuncA = GetStrategyFunc(select1);
            StrategyFunc strategyFuncB = GetStrategyFunc(select2);

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n--- MAÇ: {CharacterNames[select1 - 1]} ({strategyA.Length} Tur) vs {CharacterNames[select2 - 1]} ({strategyB.Length} Tur) ---");
            Console.ResetColor();
            Console.WriteLine("\nTUR\tHAMLE A\t\tHAMLE B\t\tTOPLAM A\tTOPLAM B");
            Console.WriteLine(new string('-', 70));

            for (int i = 0; i < strategyA.Length; i++)
            {
                // 1. Hamleleri Hesapla
                bool moveA = strategyFuncA(strategyA, strategyB, i, _grimTriggerA, _grimTriggerB);
                bool moveB = strategyFuncB(strategyB, strategyA, i, _grimTriggerB, _grimTriggerA);

                strategyA[i] = moveA;
                strategyB[i] = moveB;

                // 2. Hain Hafıza Durumunu Güncelle
                if (select1 == 4 && moveB == false) _grimTriggerA = true;
                if (select2 == 4 && moveA == false) _grimTriggerB = true;

                // 3. Puanı Hesapla ve Güncelle
                int scoreDeltaA, scoreDeltaB;
                CalculatePayoff(moveA, moveB, out scoreDeltaA, out scoreDeltaB);
                scoreA += scoreDeltaA;
                scoreB += scoreDeltaB;

                // 4. Çıktıyı Yazdır (Renkli)
                WriteTurnOutput_1v1(i + 1, moveA, moveB, scoreA, scoreB, scoreDeltaA, scoreDeltaB);
            }

            Console.WriteLine(new string('=', 70));
            ShowFinalResult_1v1(CharacterNames[select1 - 1], scoreA, CharacterNames[select2 - 1], scoreB);
        }

        // Tur çıktısını renklendirerek yazdırır (1v1)
        private static void WriteTurnOutput_1v1(int turn, bool moveA, bool moveB, int totalA, int totalB, int deltaA, int deltaB)
        {
            Console.Write($"{turn}\t");

            // Hamle A
            Console.ForegroundColor = moveA ? ConsoleColor.DarkGreen : ConsoleColor.Red;
            Console.Write(moveA ? "TRUE (C)" : "FALSE (D)");
            Console.ResetColor();

            Console.Write("\t");

            // Hamle B
            Console.ForegroundColor = moveB ? ConsoleColor.DarkGreen : ConsoleColor.Red;
            Console.Write(moveB ? "TRUE (C)" : "FALSE (D)");
            Console.ResetColor();

            // Skor A
            Console.Write($"\t{totalA} ({FormatDelta(deltaA)})");

            // Skor B
            Console.Write($"\t{totalB} ({FormatDelta(deltaB)})");

            Console.WriteLine();
        }

        // Nihai sonucu gösterir (1v1)
        private static void ShowFinalResult_1v1(string nameA, int scoreA, string nameB, int scoreB)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- OYUN SONUÇLARI ---");
            Console.WriteLine($"Toplam Skor A ({nameA}): {scoreA}");
            Console.WriteLine($"Toplam Skor B ({nameB}): {scoreB}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Magenta;
            if (scoreA > scoreB)
            {
                Console.WriteLine($"\nGENEL KAZANAN: {nameA}!");
            }
            else if (scoreB > scoreA)
            {
                Console.WriteLine($"\nGENEL KAZANAN: {nameB}!");
            }
            else
            {
                Console.WriteLine("\nSONUÇ: BERABERLİK!");
            }
            Console.ResetColor();

            Console.WriteLine("\nYeni bir simülasyon başlatmak için bir tuşa basın...");
            Console.ReadKey();
            StartMenu(); // Menüye geri dön
        }

        #endregion

        #region EVRIM SIMÜLASYONU VE ANA AKIŞ METOTLARI
        // ... (RunSimulation, InitialPopulationSetup, RunTournament, PlayMatch, EvolvePopulation, ShowPopulationState metotları değişmedi, sadece Evrimsel mod için kullanılacak)

        public static void RunSimulation()
        {
            Console.Clear();
            KarakterAciklamalariniGoster();

            // Popülasyonu başlat
            InitialPopulationSetup();

            int generation = 1;
            int totalPopulation = _population.Sum(c => c.Count);

            // Simülasyon döngüsü
            while (_population.Count(c => c.Count > 0) > 1 && generation <= 100) // Tek tip kalana kadar veya 100 jenerasyon
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n================== JENERASYON {generation} (POPÜLASYON: {totalPopulation}) ==================");
                Console.ResetColor();
                ShowPopulationState(generation);

                Console.WriteLine("\nTurnuva Başlıyor... Tüm tipler birbiriyle {0} tur oynayacak.", TUR_SAYISI);
                Console.WriteLine("Devam etmek için bir tuşa basın.");
                Console.ReadKey();

                // 1. Turnuvayı Koştur ve Puanları Topla
                RunTournament();

                // 2. Popülasyonu Evrimleştir
                EvolvePopulation();

                generation++;
            }

            Console.Clear();
            ShowPopulationState(generation);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n================== EVRİM SONA ERDİ ==================");
            Console.WriteLine("Popülasyon Dengeye Ulaştı veya Maksimum Jenerasyona Gelindi.");
            Console.ResetColor();
            Console.WriteLine("\nYeni bir simülasyon başlatmak için bir tuşa basın...");
            Console.ReadKey();
            StartMenu(); // Menüye geri dön
        }

        // Kullanıcıdan başlangıç popülasyonunu alan metot
        public static void InitialPopulationSetup()
        {
            _population.Clear();
            for (int i = 0; i < CharacterNames.Length; i++)
            {
                _population.Add(new KarakterDurumu { Id = i + 1, Name = CharacterNames[i], Strategy = GetStrategyFunc(i + 1) });
            }

            Console.WriteLine("\n" + new string('-', 60));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Lütfen her karakter tipinden kaç tane olacağını girin (Toplam {CharacterNames.Length} tip var).");
            Console.WriteLine("Boş bırakılanlar 0 kabul edilecektir.");
            Console.ResetColor();

            int total = 0;
            for (int i = 0; i < _population.Count; i++)
            {
                Console.Write($"({i + 1}) {CharacterNames[i]} adedi: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int count) && count >= 0)
                {
                    _population[i].Count = count;
                    total += count;
                }
                else
                {
                    _population[i].Count = 0;
                }
            }

            if (total == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Popülasyon sıfır olamaz. Varsayılan olarak 10 Kopyacı ve 10 Sinsi ile başlatılıyor.");
                Console.ResetColor();
                _population[0].Count = 10; // Kopyacı
                _population[2].Count = 10; // Sinsi
            }
        }

        // Turnuva Mantığı: Her karakter tipi diğer tüm tiplerle karşılaşır
        public static void RunTournament()
        {
            // Puanları sıfırla
            _population.ForEach(c => c.TotalScore = 0);

            var activeTypes = _population.Where(c => c.Count > 0).ToList();

            // Round-Robin Turnuvası: Her tip diğer her tiple oynar (kendiyle dahil)
            for (int i = 0; i < activeTypes.Count; i++)
            {
                for (int j = 0; j < activeTypes.Count; j++)
                {
                    KarakterDurumu typeA = activeTypes[i];
                    KarakterDurumu typeB = activeTypes[j];

                    // Kopyacı vs Sinsi (typeA vs typeB)
                    // Tüm maçlar aynı Tur Sayısı (TUR_SAYISI) ile oynanır.

                    int scoreA, scoreB;
                    PlayMatch(typeA, typeB, out scoreA, out scoreB);

                    // Ağırlıklı Puan Ekleme:
                    // A'nın puanı, B'nin popülasyon büyüklüğü ile çarpılır.
                    typeA.TotalScore += (long)scoreA * typeB.Count;

                    // B'nin puanı, A'nın popülasyon büyüklüğü ile çarpılır.
                    typeB.TotalScore += (long)scoreB * typeA.Count;
                }
            }
        }

        // İki karakter tipi arasındaki tek bir maçı simüle eder (TUR_SAYISI kez)
        public static void PlayMatch(KarakterDurumu typeA, KarakterDurumu typeB, out int finalScoreA, out int finalScoreB)
        {
            // Maç için skor ve hafıza sıfırlama
            int matchScoreA = 0;
            int matchScoreB = 0;

            // Hain Hafıza durumları sıfırlanır
            _grimTriggerA = false;
            _grimTriggerB = false;

            // Maç hamle hafızaları
            bool[] historyA = new bool[TUR_SAYISI];
            bool[] historyB = new bool[TUR_SAYISI];

            for (int i = 0; i < TUR_SAYISI; i++)
            {
                // Hamleleri Hesapla
                // Bu kısım, KarakterDurumu'nun Strategy delegate'ini doğru şekilde çağırır.
                bool moveA = typeA.Strategy(historyA, historyB, i, _grimTriggerA, _grimTriggerB);
                bool moveB = typeB.Strategy(historyB, historyA, i, _grimTriggerB, _grimTriggerA);

                historyA[i] = moveA;
                historyB[i] = moveB;

                // Hain Hafıza Durumunu Güncelle
                if (typeA.Id == 4 && moveB == false) _grimTriggerA = true;
                if (typeB.Id == 4 && moveA == false) _grimTriggerB = true;

                // Puanı Hesapla
                int scoreDeltaA, scoreDeltaB;
                CalculatePayoff(moveA, moveB, out scoreDeltaA, out scoreDeltaB);
                matchScoreA += scoreDeltaA;
                matchScoreB += scoreDeltaB;
            }

            finalScoreA = matchScoreA;
            finalScoreB = matchScoreB;
        }

        // Popülasyon Evrimi Mantığı (En düşük 5 tipin yerine en yüksek 5 tip gelir)
        public static void EvolvePopulation()
        {
            // Sadece yaşayan karakter tiplerini al
            var scoringTypes = _population.Where(c => c.Count > 0).ToList();

            if (!scoringTypes.Any()) return;

            // Ortalama puanı hesapla
            int totalActiveCount = scoringTypes.Sum(c => c.Count);

            var evolvedTypes = scoringTypes
                .Select(c => new
                {
                    Karakter = c,
                    AverageScore = (c.TotalScore > 0 && c.Count > 0 && totalActiveCount > 0)
                        ? (double)c.TotalScore / (c.Count * totalActiveCount)
                        : 0.0
                })
                .OrderByDescending(x => x.AverageScore)
                .ToList();

            // En düşük 5 (Kaybeden) ve En yüksek 5 (Kazanan) tipi bul
            int topCount = evolvedTypes.Count / 2;
            if (evolvedTypes.Count % 2 != 0) topCount++;

            var winners = evolvedTypes.Take(topCount).ToList();
            var losers = evolvedTypes.Skip(topCount).ToList();

            int totalPopulation = _population.Sum(c => c.Count); // Önceki toplam popülasyon

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n--- ELENENLER (LOSERS) ---");
            Console.ResetColor();

            // 1. Kaybedenleri Elimize Al
            int totalLoserCount = losers.Sum(l => l.Karakter.Count);
            foreach (var loser in losers)
            {
                Console.WriteLine($"\tELENDİ: {loser.Karakter.Name} ({loser.Karakter.Count} adet). Ortalama Skor: {loser.AverageScore:F2}");
                // Kaybedenin popülasyonunu sıfırla
                _population.First(c => c.Id == loser.Karakter.Id).Count = 0;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n--- ÇOĞALANLAR (WINNERS) ---");
            Console.ResetColor();

            // 2. Kazananların Popülasyonunu Artır (Kaybedenlerin yerlerini kazananların ortalama skor oranına göre paylaştır)
            double totalWinnerAverageScore = winners.Sum(w => w.AverageScore);

            foreach (var winner in winners)
            {
                // Kazanılan yeni yer sayısını belirle (Kaybedenlerin toplam yeri üzerinden)
                double scoreRatio = (totalWinnerAverageScore > 0) ? (winner.AverageScore / totalWinnerAverageScore) : 0;
                int newCount = (int)Math.Round(scoreRatio * totalLoserCount);

                KarakterDurumu currentWinner = _population.First(c => c.Id == winner.Karakter.Id);
                currentWinner.Count += newCount;

                // Popülasyon detaylarını yazdır
                Console.WriteLine($"\tÇOĞALDI: {currentWinner.Name} ({currentWinner.Count} adet). Ortalama Skor: {winner.AverageScore:F2}");
            }

            // Toplam Popülasyonun Korunduğunu Garanti Etmek için Son Kontrol
            int newTotal = _population.Sum(c => c.Count);

            // Yuvarlamadan kaynaklanan farkı en büyük kazanana ekle (basitleştirme)
            if (newTotal != totalPopulation)
            {
                // Mevcut popülasyonda en yüksek ortalama puana sahip olanı bularak farkı ona ekle
                var topWinner = _population.FirstOrDefault(c => c.Id == winners.First().Karakter.Id);
                if (topWinner != null)
                {
                    topWinner.Count += (totalPopulation - newTotal);
                }
            }
        }

        // Popülasyonun anlık durumunu gösterir
        public static void ShowPopulationState(int generation)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n--- JENERASYON {generation} SONU POPÜLASYON DAĞILIMI ---");
            Console.ResetColor();

            Console.WriteLine("ID\tKARAKTER\t\tADET\tTOPLAM SKOR\tORT. SKOR");
            Console.WriteLine(new string('-', 60));

            var activeTypes = _population.Where(c => c.Count > 0).ToList();
            int totalCount = activeTypes.Sum(c => c.Count);

            foreach (var type in activeTypes.OrderByDescending(c => (double)c.TotalScore / (c.Count * totalCount)))
            {
                // Ortalama Skor Hesaplama
                double averageScore = (type.Count > 0 && totalCount > 0)
                    ? (double)type.TotalScore / (type.Count * totalCount)
                    : 0.0;

                Console.WriteLine($"{type.Id}\t{type.Name.PadRight(15)}\t{type.Count}\t{type.TotalScore}\t\t{averageScore:F2}");
            }

            if (activeTypes.Count == 1)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nEVRİMSEL KAZANAN: {activeTypes.First().Name} ({activeTypes.First().Count} adet)!");
                Console.ResetColor();
            }
        }

        #endregion

        #region STRATEJI VE PUANLAMA METOTLARI
        // Puan hesaplama matrisi
        private static void CalculatePayoff(bool hamleA, bool hamleB, out int scoreA, out int scoreB)
        {
            // True = İşbirliği (C), False = İhanet (D)
            if (hamleA && hamleB) // C, C (Orta Kazanç)
            {
                scoreA = 2; scoreB = 2;
            }
            else if (hamleA && !hamleB) // C, D (Sömürülme: A kaybetti, B kazandı)
            {
                scoreA = -1; scoreB = 3;
            }
            else if (!hamleA && hamleB) // D, C (Sömürme: A kazandı, B kaybetti)
            {
                scoreA = 3; scoreB = -1;
            }
            else // !hamleA && !hamleB // D, D (Karşılıklı Kayıp)
            {
                scoreA = 0; scoreB = 0;
            }
        }

        // Karakter ID'sini alıp ilgili strateji fonksiyonunu döndürür
        private static StrategyFunc GetStrategyFunc(int id)
        {
            return id switch
            {
                1 => Kopyaci,
                2 => Poncik,
                3 => Sinsi,
                4 => HainHafiza,
                5 => SansliCimbom,
                6 => AffetmezAyna,
                7 => Somurucu,
                8 => Firsatci,
                9 => Grupcu,
                10 => Intikamci,
                _ => (ownHistory, oppHistory, i, a, b) => false, // Varsayılan: Sinsi
            };
        }

        // Karakterlerin hamlelerini hesaplayan fonksiyonlar
        // ownHistory: Kendi geçmiş hamleleri, oppHistory: Rakibin geçmiş hamleleri
        // i: Mevcut tur, isGrimTriggered: Kendi Hain Hafıza durumu

        // 1. Kopyacı (Tit-for-Tat)
        public static bool Kopyaci(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            if (i == 0) return true; // İlk hamle: İşbirliği (True)
            return oppHistory[i - 1]; // Rakibin son hamlesini kopyala
        }

        // 2. Ponçik (Always Cooperate)
        public static bool Poncik(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            return true; // Her zaman İşbirliği (True)
        }

        // 3. Sinsi (Always Defect)
        public static bool Sinsi(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            return false; // Her zaman İhanet (False)
        }

        // 4. Hain Hafıza (Grim Trigger)
        public static bool HainHafiza(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            if (i == 0) return true; // Başlangıç True

            // Eğer daha önce rakip ihanet ettiyse (isGrimTriggered true ise), hep False.
            if (isGrimTriggered) return false;

            // Eğer hiç ihanet edilmediyse, True devam et.
            return true;
        }

        // 5. Şanslı Cimbom (Random)
        public static bool SansliCimbom(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            // Next(0, 2) ya 0 ya da 1 döndürür. 1 olması %50 ihtimaldir.
            return _rnd.Next(2) == 1;
        }

        // 6. Affetmez Ayna (Tit-for-Tat with Forgiveness)
        public static bool AffetmezAyna(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            if (i == 0) return true;

            if (oppHistory[i - 1] == false) // Rakip ihanet ettiyse (False)
            {
                // %20 ihtimalle affet (True yap), %80 ihtimalle kopyala (False yap)
                // Next(1, 6) 1, 2, 3, 4, 5 döndürür. 5 gelirse affetmiş olur.
                return _rnd.Next(1, 6) == 5;
            }

            return oppHistory[i - 1]; // Rakip True yaptıysa, True yap
        }

        // 7. Sömürücü (Tester / Explorer)
        public static bool Somurucu(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            if (i == 0) return false; // Başlangıç False (Test)

            // Rakip hiç ihanet etmediyse (hep True), sömürmeye (False) devam et.
            bool oppAlwaysCooperated = true;
            for (int j = 0; j < i; j++)
            {
                if (oppHistory[j] == false)
                {
                    oppAlwaysCooperated = false;
                    break;
                }
            }

            if (oppAlwaysCooperated)
            {
                // Rakip her zaman işbirliği yaptıysa, sömürmeye devam et
                return false;
            }
            else
            {
                // Rakip tepki verdiyse (bir kere bile olsa ihanet ettiyse), Kopyacı ol.
                return oppHistory[i - 1];
            }
        }

        // 8. Fırsatçı (Joss)
        public static bool Firsatci(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            if (i == 0) return true; // Başlangıç True (Kopyacı gibi)

            // Rakip İhanet ettiyse, kopyala (False yap)
            if (oppHistory[i - 1] == false)
            {
                return false;
            }

            // Rakip İşbirliği yaptıysa (True): 
            // %80 ihtimalle True yap, %20 ihtimalle False yap (Fırsatçı hamlesi)
            // Next(1, 6) -> 1, 2, 3, 4, 5
            if (_rnd.Next(1, 6) == 5) // %20 ihtimal (sadece 5 gelirse)
            {
                return false; // İhanet et (Fırsatçı hamlesi)
            }

            return true; // İşbirliğine devam et (Kopyacı gibi)
        }

        // 9. Grupçu (Simple Majority)
        public static bool Grupcu(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            if (i == 0) return true; // Başlangıç True

            // Tüm geçmiş hamleleri (kendisi ve rakip) birleştir
            int trueCount = 0;
            int falseCount = 0;

            for (int j = 0; j < i; j++)
            {
                if (ownHistory[j]) trueCount++; else falseCount++;
                if (oppHistory[j]) trueCount++; else falseCount++;
            }

            if (trueCount > falseCount) return true;
            if (falseCount > trueCount) return false;

            // Beraberlik durumunda True (varsayılan)
            return true;
        }

        // 10. İntikamcı (Pavlov)
        public static bool Intikamci(bool[] ownHistory, bool[] oppHistory, int i, bool isGrimTriggered, bool isOppGrimTriggered)
        {
            if (i == 0) return true; // Başlangıç True

            // Önceki turun sonucunu belirle: Kazanma durumu (ödül)
            bool previousOwnMove = ownHistory[i - 1];
            bool previousOppMove = oppHistory[i - 1];

            int ownScoreDelta = 0;
            int oppScoreDelta = 0;
            CalculatePayoff(previousOwnMove, previousOppMove, out ownScoreDelta, out oppScoreDelta);

            // Eğer kazandıysa (Skor 2 veya 3 ise, yani sömürdü veya karşılıklı işbirliği yaptı)
            if (ownScoreDelta >= 2)
            {
                // Kazanmayı ödüllendir: Aynı eylemi tekrar et
                return previousOwnMove;
            }
            else
            {
                // Kaybettiyse (Skor 0 veya -1 ise), cezalandır: Eylemi değiştir
                return !previousOwnMove;
            }
        }

        #endregion
    }
}