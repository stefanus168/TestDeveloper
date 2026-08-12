using System;
using System.Linq;
using TestDeveloper;
using Xunit;

namespace TestData;

public class JadwalProduksiTests
{
    [Fact]
    public void Sample()
    {
        var rencana = new[] { 4, 5, 1, 7, 6, 4, 0 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(
            new[] { 4, 5, 4, 5, 5, 4, 0 },
            hasil);
    }

    [Fact]
    public void TotalHabisDibagi()
    {
        var rencana = new[] { 2, 4, 6, 0 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(
            new[] { 4, 4, 4, 0 },
            hasil);
    }

    [Fact]
    public void TotalBersisa()
    {
        var rencana = new[] { 5, 2, 2 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(
            new[] { 3, 3, 3 },
            hasil);
    }

    [Fact]
    public void SemuaNol()
    {
        var rencana = new[] { 0, 0, 0 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(
            new[] { 0, 0, 0 },
            hasil);
    }

    [Fact]
    public void SatuSlotAktif()
    {
        var rencana = new[] { 0, 10, 0 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(
            new[] { 0, 10, 0 },
            hasil);
    }

    [Fact]
    public void NilaiSama_PrioritasSlotAwal()
    {
        var rencana = new[] { 5, 5, 1 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(
            new[] { 4, 4, 3 },
            hasil);
    }

    [Fact]
    public void InputNegatif()
    {
        var rencana = new[] { 5, -1, 3 };

        Assert.Throws<ArgumentException>(() =>
            JadwalProduksi.Seimbangkan(rencana));
    }

    [Fact]
    public void TotalTidakBerubah()
    {
        var rencana = new[] { 4, 5, 1, 7, 6, 4, 0 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(rencana.Sum(), hasil.Sum());
    }

    [Fact]
    public void SlotNolTetapNol()
    {
        var rencana = new[] { 4, 5, 1, 7, 6, 4, 0 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(0, hasil[6]);
    }

    [Fact]
    public void SelisihMaksimalSatu()
    {
        var rencana = new[] { 4, 5, 1, 7, 6, 4, 0 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        var aktif = hasil.Where(x => x > 0);

        Assert.True(aktif.Max() - aktif.Min() <= 1);
    }

    [Fact]
    public void InputNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JadwalProduksi.Seimbangkan(null));
    }

    [Fact]
    public void SatuData()
    {
        var rencana = new[] { 10 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(new[] { 10 }, hasil);
    }

    [Fact]
    public void BanyakSlotNol()
    {
        var rencana = new[] { 0, 0, 10, 0, 0 };

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        Assert.Equal(
            new[] { 0, 0, 10, 0, 0 },
            hasil);
    }
}