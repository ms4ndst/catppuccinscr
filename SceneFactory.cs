namespace CatppuccinCoast;

public static class SceneFactory
{
    public static IScene Create(AppSettings s, double w, double h) => s.Scene switch
    {
        "forest" => new ForestScene(s, w, h),
        "peaks"  => new PeaksScene(s, w, h),
        "lofi"   => new LofiScene(s, w, h),
        _        => new CoastScene(s, w, h),
    };
}
