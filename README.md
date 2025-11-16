

⚔️ Oyun Teorisi Simülasyonu (C#)
Bu uygulama, Tekrarlı Mahkum İkilemi stratejilerinin rekabetini ve evrimini inceleyen bir C# konsol simülasyonudur. Amacımız, rasyonel karakterlerin zaman içinde nasıl hayatta kaldığını gözlemlemektir.



⚙️ Simülasyon Modları
Uygulama, iki farklı analiz türünü destekler:
Tekli Maç (1v1): Kullanıcının seçtiği iki karakteri, belirlenen tur sayısı boyunca karşılaştırır ve hamle/skor detaylarını gösterir.
Evrimsel Turnuva: Kullanıcı tarafından belirlenen başlangıç popülasyon dağılımına göre, stratejilerin jenerasyonlar içinde nasıl evrildiğini gözlemler.



🧬 Evrim Mekanizması
Evrimsel mod, Ortalama Puan ilkesine göre çalışır:
Tüm tipler kendi adetlerine göre ağırlıklandırılmış maçlar yapar.
En düşük ortalama puana sahip alt %50 elenir.
Elenenlerin yerini, en yüksek ortalama puana sahip üst %50'nin kopyaları alır ve popülasyon evrimleşir.



📊 Puan Matrisi (Kazançlar)
Puanlama, Mahkum İkilemi kurallarına tabidir (A/B sırasıyla):
True/True (İşbirliği): +2, +2 (Orta Kazanç)
True/False (Sömürü): -1, +3 (Sömürülme)
False/True (Sömürme): +3, -1 (Sömürme)
False/False (İhanet): 0, 0 (Karşılıklı Kayıp)



🦸 10 Temel Strateji Özeti
Kopyacı (Tit-for-Tat): Rakibin önceki hamlesini kopyalar.
Ponçik (Always True): Her zaman İşbirliği yapar.
Sinsi (Always False): Her zaman İhanet eder.
Hain Hafıza (Grim Trigger): Rakip bir kez False yaparsa, sonsuza dek False.
Şanslı Cimbom (Random): Her turda rastgele karar verir.
Affetmez Ayna: Kopyacı gibidir, nadiren ihaneti affeder.
Sömürücü (Tester): Başlangıç False. Rakip tepki vermezse sömürür, tepki verirse kopyacı olur.
Fırsatçı (Joss): Kopyacı stratejisi uygularken arada bir sürpriz False yapar.
Grupçu (Majority): Geçmişteki tüm hamlelerin çoğunluğuna uyar.
İntikamcı (Pavlov): Kazandıysa aynı eylemi tekrar eder, kaybettiyse değiştirir.




<img width="1776" height="871" alt="Image" src="https://github.com/user-attachments/assets/ec405c62-33a8-4bfb-880e-a3a2caf19d7e" />
<img width="607" height="178" alt="Image" src="https://github.com/user-attachments/assets/776e4699-2eab-43f7-b05e-f54ba1a6b0bc" />
<img width="1642" height="683" alt="Image" src="https://github.com/user-attachments/assets/24566341-80a4-4a0f-85fe-15df1b7999b0" />
<img width="980" height="472" alt="Image" src="https://github.com/user-attachments/assets/572d84a7-9506-4459-80f2-670397bb9518" />
<img width="1059" height="362" alt="Image" src="https://github.com/user-attachments/assets/391e451e-2853-42c2-8d13-8e191727db8f" />
<img width="1061" height="359" alt="Image" src="https://github.com/user-attachments/assets/e69a1004-767a-434e-94be-ce4c87d22dd9" />
<img width="980" height="334" alt="Image" src="https://github.com/user-attachments/assets/fddb6e10-0f64-4dcc-b265-493aec2fa2cc" />
