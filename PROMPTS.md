# LLM Kullanım Dokümantasyonu

## Özet

| Bilgi | Değer |
|-------|-------|
| Toplam prompt sayısı | 12 |
| Kullanılan araçlar | Claude (Antigravity AI Assistant) |
| En çok yardım alınan konular | Mimari kararlar, Unity API kullanımı, Ludu Arts convention uyumu |
| Tahmini LLM ile kazanılan süre | ~4 saat |

---

## Prompt 1: Interface vs Abstract Class Tercihi

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 12:00

**Prompt:**
> Unity'de bir interaction system tasarlarken IInteractable interface mi yoksa abstract class mı kullanmalıyım? Her ikisinin de avantaj ve dezavantajları nelerdir? Ayrıca InteractionDetector için raycast mı trigger-based detection mı tercih etmeliyim? Performance ve esneklik açısından trade-off'ları açıklar mısın?

**Alınan Cevap (Özet):**
> Interface + Abstract class hybrid yaklaşımı önerildi:
> - IInteractable: Contract tanımı, farklı inheritance chain'lerde kullanılabilir
> - InteractableBase: Ortak implementasyon, boilerplate azaltır
> - Raycast: Her frame kontrol, tek target, daha precise
> - Trigger: Event-based, performance friendly ama daha karmaşık setup

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [x] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Hybrid yaklaşımı tercih ettim. Ancak önerilen basit null check yerine Ludu Arts standartlarına uygun şekilde error handling ekledim. `m_` prefix'leri ve XML documentation'ı manuel olarak ekledim çünkü convention dokümanları bunu gerektiriyor.

---

## Prompt 2: Hold Interaction için Update vs Coroutine

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 12:45

**Prompt:**
> Chest gibi hold interaction gerektiren bir nesne için progress tracking implementasyonu yapıyorum. Update metodunda sürekli kontrol mü yoksa Coroutine ile async yaklaşım mı daha uygun? Delta time accumulation için edge case'ler neler olabilir?

**Alınan Cevap (Özet):**
> Update yaklaşımı önerildi:
> - Daha kontrollü, her frame state check edilebilir
> - Input release anında hemen cancel edilebilir
> - Coroutine: Daha clean ama cancel handling zorlaşabilir

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Update yaklaşımı benim düşündüğüm çözümle uyuşuyordu. InteractionDetector'da HandleHoldInteraction metodu bu mantığı implemente ediyor.

---

## Prompt 3: Rotation Animation için Quaternion Kullanımı

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 13:30

**Prompt:**
> Door ve Chest için smooth rotation animation implementasyonu gerekiyor. Quaternion.Slerp ile Quaternion.RotateTowards arasındaki fark nedir? Pivot point door'un kenarında olması gerekiyorsa (hinge point) nasıl bir hierarchy setup yapmalıyım?

**Alınan Cevap (Özet):**
> - Slerp: Easing effect, başta yavaş sonda yavaş
> - RotateTowards: Sabit hız, lineer hareket
> - Pivot: Empty parent GameObject, transform at hinge point

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [x] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Slerp tercih ettim çünkü daha doğal görünüyor. Ancak pivot setup için component'e [Tooltip] attribute'u ekledim, Inspector'da kullanıcıya rehberlik etmesi için.

---

## Prompt 4: ScriptableObject ile Item Sistem Tasarımı

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 14:15

**Prompt:**
> Farklı key türleri (mavi, kırmızı) için ScriptableObject-based item sistemi tasarlıyorum. ItemData base class mı yoksa doğrudan KeyData mı kullanmalıyım? Gelecekte farklı item türleri (weapon, potion) eklenebilir diye extensible olmasını istiyorum.

**Alınan Cevap (Özet):**
> Inheritance hierarchy önerildi:
> - ItemData (base): itemName, icon, description
> - KeyData (derived): keyId, keyColor
> - CreateAssetMenu attribute ile factory oluşturma

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Extensible yapı gelecek için mantıklı. Ludu Arts naming convention'a göre asset'leri SO_Key_Blue şeklinde isimlendirdim.

---

## Prompt 5: Inventory Slot Yönetimi - Array vs List

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 15:00

**Prompt:**
> Hotbar UI için sabit slot sayısı olan bir inventory yapıyorum (4 slot). Player item drop edince slot'lar kaymayı mı yoksa boş mu kalmalı? UX açısından hangisi daha iyi? List kullanınca slot shifting sorunu yaşadım, bunu nasıl çözerim?

**Alınan Cevap (Özet):**
> Fixed-size array yaklaşımı önerildi:
> - Slot'lar shift etmesin, boş kalsın
> - KeyData[] m_Slots = new KeyData[4]
> - Null slot = empty slot

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [x] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> LLM'in ilk önerisinde List kullanılıyordu, bu shifting sorununa yol açtı. Kendi başıma array çözümünü düşündükten sonra LLM'den confirmation aldım. m_MaxSlots constant'ı k_ prefix ile tanımladım.

---

## Prompt 6: Event-Driven Communication Pattern

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 15:45

**Prompt:**
> Switch'in bir door'u tetiklemesi gerekiyor ama tight coupling istemiyorum. UnityEvent mi yoksa C# Action/event mi kullanmalıyım? Inspector'dan bağlanabilir olması lazım.

**Alınan Cevap (Özet):**
> UnityEvent tercih edilmeli:
> - Inspector'dan method bağlama
> - Designer-friendly
> - SerializeField ile persist
> - C# Action: Code-only senaryolar için

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> UnityEvent sayesinde Switch.OnSwitchOn event'ine Door.Open() metodunu Inspector'dan bağlayabildim. Ludu Arts dokümantasyonunda event-based connection bonus özellik olarak belirtilmişti.

---

## Prompt 7: UI Canvas ve Cursor Lock State

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 16:30

**Prompt:**
> ChestLootUI açıldığında Time.timeScale = 0 ayarladım ama kamera hala hareket ediyor. FirstPersonController muhtemelen unscaledDeltaTime kullanıyor. Cursor.lockState ve Time.timeScale birlikte nasıl yönetilmeli?

**Alınan Cevap (Özet):**
> - Cursor.lockState = CursorLockMode.None
> - Cursor.visible = true
> - Player controller component'i disable et
> - FindObjectsByType ile dynamic bulma

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [x] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Player controller disable etme çözümü doğruydu. Ancak direct reference yerine runtime'da type name ile bulma yaklaşımını kullandım, bu şekilde ChestLootUI component'i reusable kaldı.

---

## Prompt 8: Progress Bar Fill Amount Animation

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 17:00

**Prompt:**
> Hold interaction için circular progress bar yapmak istiyorum. Image component'in fillAmount property'si nasıl animate edilir? OnHoldProgress callback'inden gelen 0-1 arası değeri doğrudan kullanabilir miyim?

**Alınan Cevap (Özet):**
> Image.fillAmount doğrudan progress değerine set edilebilir:
> - Image type: Filled
> - Fill method: Radial 360
> - m_ProgressBarFill.fillAmount = progress

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Unity'nin built-in fill özelliği beklendiği gibi çalıştı. InteractionDetector'dan gelen event'i InteractionPromptUI'da subscribe edip UpdateProgressBar metodunda kullandım.

---

## Prompt 9: Ludu Arts Convention Uyumu Kontrolü

**Araç:** Claude (Antigravity)
**Tarih/Saat:** 2026-01-31 18:00

**Prompt:**
> Docs klasöründeki CSharp_Coding_Conventions.md ve Naming_Convention_Kilavuzu.md dosyalarını okudum. Projemi bu standartlara göre kontrol eder misin? Özellikle m_, s_, k_ prefix'leri ve region yapısı doğru mu?

**Alınan Cevap (Özet):**
> Kontrol sonuçları:
> - m_ prefix: ✅ Tüm private field'larda doğru
> - k_ prefix: ✅ Tüm const'larda doğru
> - s_ prefix: ✅ Static field'larda doğru
> - Region yapısı: ✅ Standard sıra
> - Asset naming: ⚠️ SO asset'leri rename gerekli

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> SO asset rename önerisini uyguladım: "Blue Key.asset" → "SO_Key_Blue.asset"

---

## LLM Hataları ve Kendi Çözümlerim

### Hata 1: Silent Null Return
**LLM Önerisi:** `if (inventory == null) return;`
**Sorun:** Ludu Arts standartları silent bypass yapılmamasını söylüyor.
**Kendi Çözümüm:** Geliştirme sırasında Debug.LogError ekledim, production build'de kaldırdım.

### Hata 2: Virtual Keyword Eksik
**LLM Önerisi:** InteractableBase'de sabit property'ler.
**Sorun:** Chest'in kendi HoldDuration değerini override etmesi gerekiyordu.
**Kendi Çözümüm:** Property'leri `virtual` yaptım.

### Hata 3: Singleton Pattern Null Check
**LLM Önerisi:** Instance null ise FindObjectOfType çağır.
**Sorun:** Deprecated method kullanımı.
**Kendi Çözümüm:** FindFirstObjectByType kullandım (Unity 2023+ recommended).

---

## Genel Değerlendirme

### LLM'in En Çok Yardımcı Olduğu Alanlar
1. **Mimari kararlar** - Interface vs Abstract, Event patterns
2. **Unity API best practices** - Quaternion kullanımı, UI Fill
3. **Problem çözümü** - Slot shifting, camera lock sorunu

### LLM Kullanımı Hakkında Düşüncelerim
> LLM'i bir pair programming partner gibi kullandım. Her öneriyi direkt kopyalamadım, önce kendi çözümümü düşündüm, sonra LLM'den confirmation veya alternatif aldım. Ludu Arts standartları özellikle naming convention ve error handling konusunda LLM çıktılarını manuel olarak düzeltmemi gerektirdi.

---

*Bu doküman Ludu Arts Unity Intern Case için hazırlanmıştır.*
