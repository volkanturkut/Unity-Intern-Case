# Interaction System - Volkan Turkut

> Ludu Arts Unity Developer Intern Case

## Proje Bilgileri

| Bilgi | Değer |
|-------|-------|
| Unity Versiyonu | 6000.0.23f1 |
| Render Pipeline | URP |
| Case Süresi | 12 saat |
| Tamamlanma Oranı | %100 |

---

## Kurulum

1. Repository'yi klonlayın:
```bash
git clone https://github.com/volkanturkut/Unity-Intern-Case.git
```

2. Unity Hub'da projeyi açın
3. `Assets/VolkanTurkutCase/Scenes/TestScene.unity` sahnesini açın
4. Play tuşuna basın

---

## Nasıl Test Edilir

### Kontroller

| Tuş | Aksiyon |
|-----|---------|
| WASD | Hareket |
| Mouse | Bakış yönü |
| E | Etkileşim |
| 1-4 | Hotbar slot seçimi |
| Q | Seçili anahtarı bırak |
| ESC | Loot panelini kapat |

### Test Senaryoları

1. **Door Test:**
   - Kapıya yaklaşın, "Press E to Open" mesajını görün
   - E'ye basın, kapı açılsın
   - Tekrar basın, kapı kapansın

2. **Key + Locked Door Test:**
   - Kilitli kapıya yaklaşın, "Locked - Key Required" mesajını görün
   - Anahtarı bulun ve toplayın
   - Kilitli kapıya geri dönün, şimdi açılabilir olmalı

3. **Switch Test:**
   - Switch'e yaklaşın ve aktive edin
   - Bağlı nesnenin (kapı/ışık vb.) tetiklendiğini görün

4. **Chest Test:**
   - Sandığa yaklaşın
   - E'ye basılı tutun, progress bar dolsun
   - Sandık açılsın ve içindeki item alınsın

---

## Mimari Kararlar

### Interaction System Yapısı

```
IInteractable (Interface)
       ↓
InteractableBase (Abstract Class)
       ↓
┌──────┼──────┬──────┐
Door  Chest  Switch  KeyPickup
```

**Neden bu yapıyı seçtim:**
> Interface + Abstract Class hybrid yaklaşımı tercih ettim. Interface sayesinde farklı inheritance chain'lerde de IInteractable kullanılabilir. Abstract class ile ortak kod tekrarını önledim ve boilerplate azalttım.

**Alternatifler:**
> Sadece interface kullanabilirdim ama bu durumda her interactable'da aynı kodu tekrar yazmam gerekecekti. Sadece abstract class kullanabilirdim ama multiple inheritance yapamayacaktım.

**Trade-off'lar:**
> Bu yaklaşımda daha fazla dosya ve abstraction katmanı var. Basit projeler için overkill olabilir ama genişletilebilirlik ve maintainability açısından avantajlı.

### Kullanılan Design Patterns

| Pattern | Kullanım Yeri | Neden |
|---------|---------------|-------|
| Observer | Event system (OnInteractableDetected, OnHoldProgress) | Loose coupling, UI güncellemesi |
| Singleton | PlayerInventory.Instance | Global erişim, tek inventory |
| Template Method | InteractableBase.Interact() | Ortak flow, özelleştirilebilir adımlar |

---

## Ludu Arts Standartlarına Uyum

### C# Coding Conventions

| Kural | Uygulandı | Notlar |
|-------|-----------|--------|
| m_ prefix (private fields) | [x] | Tüm private field'larda kullanıldı |
| s_ prefix (private static) | [x] | PlayerInventory.s_Instance |
| k_ prefix (private const) | [x] | k_DefaultHoldDuration, k_MaxHotbarSlots |
| Region kullanımı | [x] | Fields, Properties, Unity Methods, Methods |
| Region sırası doğru | [x] | Standart sıraya uyuldu |
| XML documentation | [x] | Tüm public API'lerde mevcut |
| Silent bypass yok | [x] | Hata durumları handle edildi |
| Explicit interface impl. | [ ] | Gerekli görülmedi |

### Naming Convention

| Kural | Uygulandı | Örnekler |
|-------|-----------|----------|
| P_ prefix (Prefab) | [x] | P_Door, P_Chest, P_Switch, P_Key |
| M_ prefix (Material) | [x] | M_Door_Blue, M_Door_Red, M_Key |
| T_ prefix (Texture) | [ ] | Texture kullanılmadı |
| SO isimlendirme | [x] | SO_Key_Blue, SO_Key_Red |

### Prefab Kuralları

| Kural | Uygulandı | Notlar |
|-------|-----------|--------|
| Transform (0,0,0) | [x] | Root transform sıfırlandı |
| Pivot bottom-center | [x] | Prefab'ler için uygulandı |
| Collider tercihi | [x] | Box Collider tercih edildi |
| Hierarchy yapısı | [x] | Model, Collider ayrımı yapıldı |

### Zorlandığım Noktalar
> - Chest lid animasyonu: Hold progress sırasında mı açılmalı yoksa tamamlandığında mı? Sonuçta tamamlandığında açılmasına karar verdim.
> - Time.timeScale = 0 ayarlandığında kamera hala hareket ediyordu çünkü FirstPersonController unscaledDeltaTime kullanıyor. Player controller disable ederek çözdüm.

---

## Tamamlanan Özellikler

### Zorunlu (Must Have)

- [x] Core Interaction System
  - [x] IInteractable interface
  - [x] InteractionDetector
  - [x] Range kontrolü

- [x] Interaction Types
  - [x] Instant
  - [x] Hold
  - [x] Toggle

- [x] Interactable Objects
  - [x] Door (locked/unlocked)
  - [x] Key Pickup
  - [x] Switch/Lever
  - [x] Chest/Container

- [x] UI Feedback
  - [x] Interaction prompt
  - [x] Dynamic text
  - [x] Hold progress bar
  - [x] Cannot interact feedback

- [x] Simple Inventory
  - [x] Key toplama
  - [x] UI listesi

### Bonus (Nice to Have)

- [x] Animation entegrasyonu
- [x] Sound effects (integration points hazır)
- [x] Multiple keys / color-coded
- [ ] Interaction highlight
- [ ] Save/Load states
- [x] Chained interactions

---

## Bilinen Limitasyonlar

### Tamamlanamayan Özellikler
1. Interaction Highlight - Zaman yetersizliği nedeniyle outline shader eklenmedi
2. Save/Load - PlayerPrefs ile basit implementation yapılabilirdi ama öncelik verilmedi

### Bilinen Bug'lar
1. Yok - Bilinen aktif bug bulunmamaktadır

### İyileştirme Önerileri
1. Object Pooling - Dropped key'ler için instantiate yerine pool kullanılabilir
2. Localization - Prompt mesajları için string table desteği eklenebilir

---

## Ekstra Özellikler

Zorunlu gereksinimlerin dışında eklediklerim:

1. **Hotbar UI**
   - Açıklama: 4 slotlu hotbar, toplanan anahtarlar görünür
   - Neden ekledim: Oyuncunun hangi anahtarlara sahip olduğunu görmesi için

2. **Loot UI Panel**
   - Açıklama: Chest açıldığında item detayları, collect butonu
   - Neden ekledim: Chest içeriğini görmek ve toplamak için UX iyileştirmesi

3. **Key Drop Mechanic**
   - Açıklama: Q tuşuyla seçili anahtarı bırakabilme
   - Neden ekledim: Inventory yönetimi ve realism için

4. **Held Item Display**
   - Açıklama: Seçili anahtar oyuncunun elinde görünür
   - Neden ekledim: Hangi anahtarın seçili olduğunu görsel olarak göstermek için

---

## Dosya Yapısı

```
Assets/
├── VolkanTurkutCase/
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   ├── Core/
│   │   │   │   ├── IInteractable.cs
│   │   │   │   ├── InteractableBase.cs
│   │   │   │   ├── InteractionType.cs
│   │   │   │   ├── ItemData.cs
│   │   │   │   └── KeyData.cs
│   │   │   ├── Interactables/
│   │   │   │   ├── Door.cs
│   │   │   │   ├── Chest.cs
│   │   │   │   ├── Switch.cs
│   │   │   │   └── KeyPickup.cs
│   │   │   ├── Player/
│   │   │   │   ├── InteractionDetector.cs
│   │   │   │   └── PlayerInventory.cs
│   │   │   └── UI/
│   │   │       ├── InteractionPromptUI.cs
│   │   │       ├── ChestLootUI.cs
│   │   │       ├── CrosshairUI.cs
│   │   │       ├── HotbarUI.cs
│   │   │       ├── HotbarSlot.cs
│   │   │       ├── HeldItemDisplay.cs
│   │   │       └── InventoryUI.cs
│   │   └── Editor/
│   ├── ScriptableObjects/
│   │   └── Items/
│   │       ├── SO_Key_Blue.asset
│   │       └── SO_Key_Red.asset
│   ├── Prefabs/
│   │   ├── Interactables/
│   │   └── UI/
│   ├── Materials/
│   └── Scenes/
│       └── TestScene.unity
├── Docs/
│   ├── CSharp_Coding_Conventions.md
│   ├── Naming_Convention_Kilavuzu.md
│   └── Prefab_Asset_Kurallari.md
├── README.md
├── PROMPTS.md
└── .gitignore
```

---

## İletişim

| Bilgi | Değer |
|-------|-------|
| Ad Soyad | Volkan Turkut |
| E-posta | volkan.trkt@gmail.com |
| LinkedIn | linkedin.com/in/volkanturkut/ |
| GitHub | github.com/volkanturkut |

---

*Bu proje Ludu Arts Unity Developer Intern Case için hazırlanmıştır.*
