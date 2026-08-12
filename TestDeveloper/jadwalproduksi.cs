using System;
using System.Collections.Generic;
using System.Linq;

namespace TestDeveloper;

public static class JadwalProduksi
{
    public static List<int> Seimbangkan(IReadOnlyList<int> rencana)
    {
        if (rencana == null)
            throw new ArgumentNullException(nameof(rencana));

        if (rencana.Any(x => x < 0))
            throw new ArgumentException("Input bilagan bulat");

        int total = rencana.Sum();

        var aktif = rencana
            .Select((nilai, i) => new { nilai, i })
            .Where(x => x.nilai > 0)
            .ToList();

        if (aktif.Count == 0)
            return rencana.Select(x => 0).ToList();

        int dasar = total / aktif.Count;
        int sisa = total % aktif.Count;

        var hasil = rencana
            .Select(x => x == 0 ? 0 : dasar)
            .ToList();

        var prioritas = aktif
            .OrderByDescending(x => x.nilai)
            .ThenBy(x => x.i)
            .Take(sisa)
            .ToList();

        foreach (var item in prioritas)
        {
            hasil[item.i]++;
        }

        return hasil;
    }
}