## Test case 1

task nomor 1
FUNCTION A

    VALIDASI input

    total = SUM seluruh nilai input

    slot = semua slot dengan nilai > 0

    IF slot kosong
        RETURN semua nilai 0
    END IF

    nilai = total / jumlah slot
    sisa = total MOD jumlah slot

    hasil = semua slot aktif kasih nilai
    slot dengan nilai awal 0 tetap 0

    urutin slot berdasarkan:
        nilai awal terbesar
        jika sama, index terkecil

    UNTUK i = 0 sampai sisa - 1
        hasil[slot[i]] = hasil[slot[i]] + 1
    END FOR

    RETURN hasil

END FUNCTION


->asumsi 
nilai selalu bulat
input angka
slot variasi

->kompleksitas
mencari slot aktif 
hitung totalnya
sorting 
bagiin sisanya 

task nomor 5 
Mencegah error ketika function menerima input null.
Mencegah kesalahan pembagian atau perubahan nilai ketika hanya ada satu
slot aktif
Mencegah slot yang tidak aktif ikut mendapatkan hasil

## Test case 2

