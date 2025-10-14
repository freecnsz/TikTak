# TikTak - Profesyonel Sunum Sayacı

**TikTak**, kongre, seminer ve sunum organizasyonları için özel olarak tasarlanmış modern bir zamanlayıcı uygulamasıdır. Kullanıcı dostu arayüzü ve güçlü özellikleri ile profesyonel etkinliklerde zaman yönetimini kolaylaştırır.

## ✨ Temel Özellikler

- **🎤 Sunum Optimizasyonu**: Kongre ve seminerlerde kullanım için özel tasarım
- **🎨 Çoklu Tema Desteği**: 5 farklı renk teması (Koyu, Açık, Koyu Mavi, Açık Mavi, Yeşil)
- **🌍 Çok Dilli Mesajlar**: Türkçe ve İngilizce "süre bitti" mesajları
- **🔔 Akıllı Bildirimler**: 5 dakika ve 1 dakika uyarı sistemi
- **🚀 Sistem Tepsisi**: Ctrl+Shift+T ile hızlı erişim
- **⚡ Global Kısayollar**: F5 (Başlat/Durdur), F7 (Sıfırla)
- **🔄 Negatif Sayım**: Süre bitince devam eden sayaç
- **🖱️ Sürüklenebilir**: Ekranda istediğiniz yere yerleştirin

## 🚀 Hızlı Başlangıç

### Sistem Gereksinimleri
- Windows 10/11
- .NET 9.0 Runtime

### Kurulum
1. Repository'yi klonlayın:
   ```bash
   git clone https://github.com/freecnsz/TikTak.git
   cd tiktak
   ```

2. Uygulamayı derleyin ve çalıştırın:
   ```bash
   dotnet build
   dotnet run
   ```

## 🎮 Kullanım Kılavuzu

### Temel Kullanım
1. **Süre Ayarlama**: İnput kutularına dakika girin
2. **Başlatma**: ▶️ butonuna basın
3. **Duraklatma**: ⏸️ butonuna basın
4. **Sıfırlama**: 🔄 butonuna basın

### Klavye Kısayolları
| Kısayol | İşlev |
|---------|-------|
| `Ctrl+Shift+T` | Göster/Gizle |
| `F5` | Başlat/Durdur |
| `F7` | Sıfırla |
| `ESC` | Kapat ve sıfırla |

### Sistem Tepsisi
- **Çift tık**: Pencereyi aç/kapat
- **Sağ tık**: Menü (Ayarlar, Hakkında, Çıkış)
- **Hover**: Mevcut durum bilgisi

## ⚙️ Konfigürasyon

Ayarlar penceresi üzerinden:
- **Tema seçimi**: 5 farklı renk kombinasyonu
- **Dil ayarı**: Türkçe/İngilizce süre bitti mesajları
- **Bildirimler**: 5dk/1dk uyarıları açma/kapama
- **Masaüstü bildirimleri**: Sistem bildirimi kontrolü

## 🏗️ Teknik Detaylar

### Mimari
- **Framework**: .NET 9.0 WPF
- **Desen**: MVVM (Model-View-ViewModel)
- **Sistem Entegrasyonu**: Windows Forms NotifyIcon
- **Veri Saklama**: JSON tabanlı ayarlar

### Proje Yapısı
```
TikTak/
├── Models/           # Veri modelleri
├── Services/         # İş mantığı servisleri
├── Windows/          # UI pencereleri
└── Resources/        # Görseller ve kaynaklar
```

## 📋 Changelog

### v1.0.0 (Ekim 2025)
- İlk sürüm yayını
- Temel zamanlayıcı özellikleri
- Çoklu tema desteği
- Sistem tepsisi entegrasyonu
- Çok dilli mesaj desteği

## Destek

- **Hata Bildirim**: [Issues](https://github.com/freecnsz/TikTak/issues)
- **Özellik İsteği**: [Discussions](https://github.com/freecnsz/TikTak/discussions)

## 📄 Lisans

Bu proje [MIT License](LICENSE) altında lisanslanmıştır.

---

**TikTak** ile profesyonel sunumlarınızda zamanı kontrol edin!