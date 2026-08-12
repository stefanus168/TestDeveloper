
## Case 1 
```text
FUNCTION Seimbangkan(rencana)

    VALIDASI input
    total = SUM seluruh nilai input
    slot_aktif = semua slot dengan nilai > 0

    IF slot_aktif kosong
        RETURN semua nilai 0
    END IF

    dasar = total / jumlah slot_aktif
    sisa = total MOD jumlah slot_aktif

    hasil = isi nilai 'dasar' untuk slot_aktif, dan 0 untuk slot_non_aktif

    urutkan slot_aktif berdasarkan:
        1. Nilai awal terbesar
        2. Jika nilai awal sama, utamakan indeks slot terkecil

    UNTUK i = 0 sampai sisa - 1
        hasil[slot_aktif[i].indeks] = hasil[slot_aktif[i].indeks] + 1
    END FOR

    RETURN hasil

END FUNCTION
```

### Asumsi & Kasus Khusus
- Nilai input selalu berupa bilangan bulat non-negatif.
- Mencegah error jika input `null` (`ArgumentNullException`).
- Slot dengan nilai awal `0` bersifat pasif dan tidak akan mendapatkan jatah alokasi beban produksi.
- Sisa pembagian dialokasikan adil kepada slot dengan beban awal terbesar terlebih dahulu.

---

## Case 2 


1.Dalam satu kali perencanaan produksi, terdapat banyak slot waktu/baris data. Memisahkan antara header transaksi (`Planning`) dan rincian slot (`PlanningSlot`) mencegah pengulangan data (redundansi). 

2. Di sistem nyata, jika koneksi internet terputus lalu pengguna mengklik tombol *submit* dua kali, atau sistem mengirim ulang request yang sama, kita tidak ingin ada 2 transaksi duplikat di database. Dengan memeriksa `RequestCode`, request ganda akan langsung mengembalikan hasil transaksi yang sudah pernah disimpan.

3. Ketika menyimpan 1 `Planning` bersama 7 `PlanningSlot`, semua data harus berhasil disimpan bersamaan. Jika terjadi kegagalan di tengah jalan (misal mati lampu atau database error pada slot ke-4), sistem akan melakukan **Rollback** otomatis sehingga tidak ada data "setengah jadi" yang tersimpan di database.

4. Sistem secara cerdas menandai slot mana yang aktif (`OriginalQuantity > 0`). Slot yang dari awal `0` tidak diberi beban produksi dan statusnya otomatis `IsActive = false`.


