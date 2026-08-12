
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
Gunakan postman untuk percobaan datanya 

http://localhost:5291/api/planning

{
  "requestCode": "REQ-001",
  "candidateToken": "TOKEN-123",
  "slots": [
    { "slotOrder": 1, "slotName": "Slot A", "originalQuantity": 4 },
    { "slotOrder": 2, "slotName": "Slot B", "originalQuantity": 5 },
    { "slotOrder": 3, "slotName": "Slot C", "originalQuantity": 1 },
    { "slotOrder": 4, "slotName": "Slot D", "originalQuantity": 7 },
    { "slotOrder": 5, "slotName": "Slot E", "originalQuantity": 6 },
    { "slotOrder": 6, "slotName": "Slot F", "originalQuantity": 4 },
    { "slotOrder": 7, "slotName": "Slot G", "originalQuantity": 0 }
  ]
}

1.Dalam satu kali perencanaan produksi, terdapat banyak slot waktu/baris data. Memisahkan antara header transaksi (`Planning`) dan rincian slot (`PlanningSlot`) mencegah pengulangan data (redundansi). 

2. Di sistem nyata, jika koneksi internet terputus lalu pengguna mengklik tombol *submit* dua kali, atau sistem mengirim ulang request yang sama, kita tidak ingin ada 2 transaksi duplikat di database. Dengan memeriksa `RequestCode`, request ganda akan langsung mengembalikan hasil transaksi yang sudah pernah disimpan.

3. Ketika menyimpan 1 `Planning` bersama 7 `PlanningSlot`, semua data harus berhasil disimpan bersamaan. Jika terjadi kegagalan di tengah jalan (misal mati lampu atau database error pada slot ke-4), sistem akan melakukan rollback otomatis sehingga tidak ada data "setengah jadi" yang tersimpan di database.

4. Sistem secara cerdas menandai slot mana yang aktif (`OriginalQuantity > 0`). Slot yang dari awal `0` tidak diberi beban produksi dan statusnya otomatis `IsActive = false`.


## Case 3 

1. 
CREATE TABLE Planning
(
    PlanningId INT IDENTITY(1,1) PRIMARY KEY,
    RequestCode VARCHAR(100) NOT NULL UNIQUE,
    CandidateToken VARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    Status VARCHAR(50) NOT NULL
);

CREATE TABLE PlanningSlot
(
    PlanningId INT NOT NULL,
    SlotOrder INT NOT NULL,
    SlotName VARCHAR(100) NOT NULL,
    OriginalQuantity INT NOT NULL,
    BalancedQuantity INT NOT NULL,
    IsActive BIT NOT NULL,

    PRIMARY KEY (PlanningId, SlotOrder),

    FOREIGN KEY (PlanningId)
        REFERENCES Planning(PlanningId)
);

2. 
INSERT INTO Planning
(RequestCode, CandidateToken, CreatedAt, Status)
VALUES
('REQ001', 'VEH-CAND001', '2026-08-01', 'COMPLETED'),
('REQ002', 'VEH-CAND002', '2026-08-02', 'COMPLETED'),
('REQ003', 'VEH-CAND003', '2026-08-03', 'COMPLETED'),
('REQ004', 'VEH-CAND004', '2026-08-04', 'COMPLETED'),
('REQ005', 'VEH-CAND005', '2026-08-05', 'COMPLETED'),
('REQ006', 'VEH-CAND006', '2026-08-06', 'COMPLETED'),
('REQ007', 'VEH-CAND007', '2026-08-07', 'PROCESSING'),
('REQ008', 'VEH-CAND008', '2026-08-08', 'FAILED'),
('REQ009', 'VEH-CAND009', '2026-08-09', 'COMPLETED'),
('REQ010', 'VEH-CAND010', '2026-08-10', 'COMPLETED');

INSERT INTO PlanningSlot
(PlanningId, SlotOrder, SlotName,
 OriginalQuantity, BalancedQuantity, IsActive)
VALUES

-- 1. Normal
(1, 1, 'Slot A', 40, 35, 1),
(1, 2, 'Slot B', 30, 35, 1),
(1, 3, 'Slot C', 30, 30, 1),

-- 2. Semua 0
(2, 1, 'Slot A', 0, 0, 1),
(2, 2, 'Slot B', 0, 0, 1),
(2, 3, 'Slot C', 0, 0, 1),

-- 3. Satu slot aktif
(3, 1, 'Slot A', 100, 100, 1),
(3, 2, 'Slot B', 0, 0, 0),
(3, 3, 'Slot C', 0, 0, 0),

-- 4. Tie
(4, 1, 'Slot A', 40, 30, 1),
(4, 2, 'Slot B', 30, 20, 1),
(4, 3, 'Slot C', 30, 50, 1),

-- 5. Total bersisa
(5, 1, 'Slot A', 40, 33, 1),
(5, 2, 'Slot B', 30, 33, 1),
(5, 3, 'Slot C', 30, 33, 1),

-- 6. Nilai besar
(6, 1, 'Slot A', 400000, 350000, 1),
(6, 2, 'Slot B', 300000, 300000, 1),
(6, 3, 'Slot C', 300000, 350000, 1),

-- 7. Processing
(7, 1, 'Slot A', 200, 150, 1),
(7, 2, 'Slot B', 150, 150, 1),
(7, 3, 'Slot C', 150, 200, 1),

-- 8. Failed
(8, 1, 'Slot A', 100, 100, 1),
(8, 2, 'Slot B', 100, 90, 1),
(8, 3, 'Slot C', 100, 110, 1),

-- 9. Normal
(9, 1, 'Slot A', 60, 60, 1),
(9, 2, 'Slot B', 70, 65, 1),
(9, 3, 'Slot C', 50, 60, 1),
(9, 4, 'Slot D', 70, 65, 1),

-- 10. Tidak sesuai
(10, 1, 'Slot A', 100, 100, 1),
(10, 2, 'Slot B', 50, 25, 0);

3.


SELECT
    p.PlanningId,

    SUM(s.OriginalQuantity) AS OriginalTotal,

    SUM(s.BalancedQuantity) AS BalancedTotal,

    CASE
        WHEN SUM(s.OriginalQuantity)
           = SUM(s.BalancedQuantity)
        THEN 1
        ELSE 0
    END AS IsTotalValid

FROM Planning p

JOIN PlanningSlot s
    ON p.PlanningId = s.PlanningId

GROUP BY p.PlanningId;

4.

SELECT
    p.RequestCode,
    p.CreatedAt,

    SUM(
        CASE
            WHEN s.IsActive = 1 THEN 1
            ELSE 0
        END
    ) AS ActiveSlotCount,

    SUM(s.OriginalQuantity) AS OriginalTotal,

    SUM(s.BalancedQuantity) AS BalancedTotal,

    p.Status

FROM Planning p

JOIN PlanningSlot s
    ON p.PlanningId = s.PlanningId

GROUP BY
    p.RequestCode,
    p.CreatedAt,
    p.Status

ORDER BY p.CreatedAt DESC;

5.
SELECT
    p.PlanningId,
    p.RequestCode

FROM Planning p

WHERE
    NOT EXISTS (
        SELECT 1
        FROM PlanningSlot s
        WHERE s.PlanningId = p.PlanningId
    )

    OR

    EXISTS (
        SELECT 1
        FROM PlanningSlot s
        WHERE s.PlanningId = p.PlanningId
          AND s.IsActive = 0
          AND s.BalancedQuantity > 0
    )

    OR

    (
        SELECT SUM(s.OriginalQuantity)
        FROM PlanningSlot s
        WHERE s.PlanningId = p.PlanningId
    )
    <>
    (
        SELECT SUM(s.BalancedQuantity)
        FROM PlanningSlot s
        WHERE s.PlanningId = p.PlanningId
    );

6.
    SELECT TOP 3
    PlanningId,
    SlotOrder,
    SlotName,
    OriginalQuantity,
    BalancedQuantity,

    ABS(BalancedQuantity - OriginalQuantity)
        AS Adjustment

    FROM PlanningSlot

    ORDER BY
    ABS(BalancedQuantity - OriginalQuantity) DESC,
    SlotOrder ASC;


7.
    BEGIN TRANSACTION;

    BEGIN TRY

    INSERT INTO Planning
    (RequestCode, CandidateToken, Status)
    VALUES
    ('REQ011', 'VEH-CAND011', 'PROCESSING');

    DECLARE @PlanningId INT = SCOPE_IDENTITY();

    INSERT INTO PlanningSlot
    (PlanningId, SlotOrder, SlotName,
     OriginalQuantity, BalancedQuantity, IsActive)
    VALUES
    (@PlanningId, 1, 'Slot A', 50, 50, 1);

    INSERT INTO PlanningSlot
    (PlanningId, SlotOrder, SlotName,
     OriginalQuantity, BalancedQuantity, IsActive)
    VALUES
    (@PlanningId, 2, 'Slot B', 50, 50, 1);

    INSERT INTO PlanningSlot
    (PlanningId, SlotOrder, SlotName,
     OriginalQuantity, BalancedQuantity, IsActive)
    VALUES
    (@PlanningId, 3, 'Slot C', 50, 50, 1);

    COMMIT;

    END TRY

    BEGIN CATCH

    ROLLBACK;

    THROW;

    END CATCH;

8.
    CREATE TABLE RebalanceRun (
    RebalanceRunId INT IDENTITY(1,1) PRIMARY KEY,
    PlanningId INT NOT NULL,
    RunVersion INT NOT NULL,
    RunDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20),

    FOREIGN KEY (PlanningId)
        REFERENCES Planning(PlanningId)
    );

9.
    CREATE INDEX IX_Planning_CreatedAt
    ON Planning(CreatedAt);

    CREATE INDEX IX_Planning_Status
    ON Planning(Status);

10.
    SELECT s.*
FROM PlanningSlot s
LEFT JOIN Planning p
    ON s.PlanningId = p.PlanningId
WHERE p.PlanningId IS NULL;

    