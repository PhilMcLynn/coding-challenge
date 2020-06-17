using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConstructionLine.CodingChallenge
{
    public class SearchEngine
    {
        private readonly List<Shirt> _shirts;
        private readonly List<SizeColorCount> _sizeColorCount;

        public SearchEngine(List<Shirt> shirts)
        {
            _shirts = shirts;

            // TODO: data preparation and initialisation of additional data structures to improve performance goes here.
            _sizeColorCount = (from shrt in _shirts
                               group shrt by new { shrt.Size, shrt.Color } into grp
                               select new SizeColorCount()
                               { Size = grp.Key.Size, Color = grp.Key.Color, Count = grp.Count() })
                           .ToList<SizeColorCount>();
        }


        public SearchResults Search(SearchOptions options)
        {
            // TODO: search logic goes here.

            // srcoptions may not be populated and so we should allow for empty options
            List<Color> colorModidifed = Color.All.ToList();
            List<Size> sizeModidifed = Size.All.ToList();
            if (options.Colors.Count > 0)
            {
                colorModidifed = (Color.All.Intersect(options.Colors)).ToList();
            }
            if (options.Sizes.Count > 0)
            {
                sizeModidifed = (Size.All.Intersect(options.Sizes)).ToList();
            }

            var qrySize = (from szclr in _sizeColorCount
                           join cm in colorModidifed on szclr.Color equals cm
                           join sm in sizeModidifed on szclr.Size equals sm
                           group szclr by szclr.Size into grp
                           select new SizeCount() { Size = grp.Key, Count = grp.Sum(t => t.Count) }).ToList<SizeCount>();

            var qryColor = (from szclr in _sizeColorCount
                            join cm in colorModidifed on szclr.Color equals cm
                            join sm in sizeModidifed on szclr.Size equals sm
                            group szclr by szclr.Color into grp
                            select new ColorCount() { Color = grp.Key, Count = grp.Sum(t => t.Count) }).ToList<ColorCount>();

            // Do this after you do the qry on the aggregated table as we can decide 
            // if it needed based on the aggregate results!
            List<Shirt> qryShrt = new List<Shirt>();
            if (qryColor.Any() || qrySize.Any())
            {
                qryShrt = (from shrt in _shirts
                           join optColors in colorModidifed on shrt.Color.Name equals optColors.Name
                           join optSize in sizeModidifed on shrt.Size.Name equals optSize.Name
                           select shrt).ToList<Shirt>();
            }

            var qrySizeFnl = (from sz in Size.All
                              join qry in qrySize on sz.Name equals qry.Size.Name into ljres
                              from subqry in ljres.DefaultIfEmpty()
                              select new SizeCount() { Size = sz, Count = subqry == null ? 0 : subqry.Count }).ToList();

            var qryColorFnl = (from clr in Color.All
                               join qry in qryColor on clr equals qry.Color into ljres
                               from subqry in ljres.DefaultIfEmpty()
                               select new ColorCount() { Color = clr, Count = subqry == null ? 0 : subqry.Count }).ToList();

            return new SearchResults
            {
                Shirts = qryShrt,
                SizeCounts = qrySizeFnl,
                ColorCounts = qryColorFnl
            };
        }
    }
}