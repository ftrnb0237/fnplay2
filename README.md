# Unity Universal Game Controller
PC ve Mobil platformları otomatik algılayan, tek dosya üzerinden çalışan Unity GameController betiği.
## 🚀 Özellikler
- **Çoklu Platform Desteği:** Android, iOS veya dokunmatik cihazlarda Mobil UI kontrollerini aktif eder, PC'de klavye kontrollerini çalıştırır.
- **Gece / Gündüz Döngüsü:** Ayarlanabilir süreye sahip dinamik ışıklandırma.
- **Hareket Sistemi:** PC'de Çift-W, mobilde UI butonu ile koşma desteği.
- **Etkileşim:** Raycast tabanlı eşya alma, taşıma ve fırlatıp bırakma.
- **El Feneri:** L tuşu veya ekran butonuyla kontrol edilir.
## 🎮 Kontroller

| Eylem | PC Klavye | Mobil UI |
| :--- | :--- | :--- |
| Hareket | W, A, S, D | Sol ekran dokunmatiği |
| Koşma | Çift-W | Koşma Butonu |
| El Feneri | L Tuşu | Işık Butonu |
| Etkileşim | F Tuşu | El Butonu |

## 🛠️ Unity Kurulumu

1. `GameController.cs` dosyasını karakter nesnesine ekleyin.
2. `Directional Light` nesnesini **Directional Light** alanına sürükleyin.
3. Mobil arayüzünü bir Canvas altında oluşturup **Mobile UI Controls** alanına bağlayın. Arayüz yalnızca mobil platformlarda görünür.
4. Harici joystick paketi gerekmez. Mobilde ekranın sol yarısına dokunup parmağınızı sürükleyerek hareket edin.
5. El feneri nesnesini **Flashlight**, karakterin önündeki taşıma noktasını **Hold Position** alanına bağlayın.
6. Koşma butonunun `Pointer Down` event’ine `SetMobileRunTrue`, `Pointer Up` event’ine `SetMobileRunFalse` ekleyin.
7. Işık butonunun `OnClick` event’ine `ToggleFlashlight`, el butonunun `OnClick` event’ine `InteractWithObject` ekleyin.

## 📦 Etkileşilebilir Nesneler

Eşya alabilmek için nesneye `Pickable` tag’i ve isteğe bağlı olarak bir `Rigidbody` ekleyin. Karakter eşyanın en fazla 3 birim önündeyken `F` tuşu veya mobil el butonu ile alır; aynı komut eşyayı bırakır ve ileri doğru fırlatır.

## ⚙️ Inspector Ayarları

- **Day Night Duration:** Bir tam gece-gündüz döngüsünün saniye cinsinden süresi.
- **Walk Speed / Run Speed:** Yürüme ve koşma hızları.
- **Difficulty:** Takipçi hızı için `Easy`, `Medium` veya `Hard` seçin.
- **Language:** Gelecekteki yerelleştirme seçenekleri için dil seçimi.

Script, Android, iOS ve dokunmatik cihazları otomatik olarak mobil kabul eder. Diğer platformlarda klavye kontrolleri kullanılır.