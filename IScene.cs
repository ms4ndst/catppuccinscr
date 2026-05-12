using System.Windows.Media;

namespace CatppuccinCoast;

public interface IScene
{
    void Update(double dt);
    void Draw(DrawingContext dc, double w, double h, double ppd);
}
