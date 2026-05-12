"""
Catppuccin Coast Screensaver v2
Windows screensaver (.scr) — all 4 Catppuccin flavors, cat mascot, real settings.
"""

import sys, math, random, ctypes, ctypes.wintypes, os, json, datetime
import pygame

# ---------------------------------------------------------------------------
# Resource path (works both in dev and PyInstaller bundle)
# ---------------------------------------------------------------------------

def resource_path(rel):
    base = getattr(sys, '_MEIPASS', os.path.dirname(os.path.abspath(__file__)))
    return os.path.join(base, rel)

# ---------------------------------------------------------------------------
# Settings
# ---------------------------------------------------------------------------

_SETTINGS_DIR  = os.path.join(os.environ.get('APPDATA', os.path.expanduser('~')), 'CatppuccinCoast')
_SETTINGS_FILE = os.path.join(_SETTINGS_DIR, 'settings.json')

DEFAULTS = {
    'flavor':            'mocha',
    'show_clock':        True,
    'show_aurora':       True,
    'show_shooting':     True,
    'show_foam':         True,
    'wave_speed':        1.0,
    'cat_size':          'medium',
}

def load_settings():
    try:
        with open(_SETTINGS_FILE) as f:
            return {**DEFAULTS, **json.load(f)}
    except Exception:
        return DEFAULTS.copy()

def save_settings(s):
    os.makedirs(_SETTINGS_DIR, exist_ok=True)
    with open(_SETTINGS_FILE, 'w') as f:
        json.dump(s, f, indent=2)

# ---------------------------------------------------------------------------
# Full four-flavor palette
# ---------------------------------------------------------------------------

PALETTES = {
    'latte': {
        'name': 'Latte', 'light': True,
        'CRUST':    (0xdc, 0xe0, 0xe8), 'MANTLE':   (0xe6, 0xe9, 0xef),
        'BASE':     (0xef, 0xf1, 0xf5), 'SURFACE0': (0xcc, 0xd0, 0xda),
        'SURFACE1': (0xbc, 0xc0, 0xcc), 'SURFACE2': (0xac, 0xb0, 0xbe),
        'OVERLAY0': (0x9c, 0xa0, 0xb0), 'TEXT':     (0x4c, 0x4f, 0x69),
        'SUBTEXT0': (0x6c, 0x6f, 0x85), 'LAVENDER': (0x72, 0x87, 0xfd),
        'BLUE':     (0x1e, 0x66, 0xf5), 'SAPPHIRE': (0x20, 0x9f, 0xb5),
        'SKY':      (0x04, 0xa5, 0xe5), 'TEAL':     (0x17, 0x92, 0x99),
        'GREEN':    (0x40, 0xa0, 0x2b), 'YELLOW':   (0xdf, 0x8e, 0x1d),
        'MAUVE':    (0x88, 0x39, 0xef), 'PINK':     (0xea, 0x76, 0xcb),
        'PEACH':    (0xfe, 0x64, 0x0b), 'MAROON':   (0xe6, 0x45, 0x53),
    },
    'frappe': {
        'name': 'Frappé', 'light': False,
        'CRUST':    (0x23, 0x26, 0x34), 'MANTLE':   (0x29, 0x2c, 0x3c),
        'BASE':     (0x30, 0x34, 0x46), 'SURFACE0': (0x41, 0x45, 0x59),
        'SURFACE1': (0x51, 0x57, 0x6d), 'SURFACE2': (0x62, 0x68, 0x80),
        'OVERLAY0': (0x73, 0x79, 0x94), 'TEXT':     (0xc6, 0xd0, 0xf5),
        'SUBTEXT0': (0xa5, 0xad, 0xce), 'LAVENDER': (0xba, 0xbb, 0xf1),
        'BLUE':     (0x8c, 0xaa, 0xee), 'SAPPHIRE': (0x85, 0xc1, 0xdc),
        'SKY':      (0x99, 0xd1, 0xdb), 'TEAL':     (0x81, 0xc8, 0xbe),
        'GREEN':    (0xa6, 0xd1, 0x89), 'YELLOW':   (0xe5, 0xc8, 0x90),
        'MAUVE':    (0xca, 0x9e, 0xe6), 'PINK':     (0xf4, 0xb8, 0xe4),
        'PEACH':    (0xef, 0x9f, 0x76), 'MAROON':   (0xea, 0x99, 0x9c),
    },
    'macchiato': {
        'name': 'Macchiato', 'light': False,
        'CRUST':    (0x18, 0x19, 0x26), 'MANTLE':   (0x1e, 0x20, 0x30),
        'BASE':     (0x24, 0x27, 0x3a), 'SURFACE0': (0x36, 0x3a, 0x4f),
        'SURFACE1': (0x49, 0x4d, 0x64), 'SURFACE2': (0x5b, 0x60, 0x78),
        'OVERLAY0': (0x6e, 0x73, 0x8d), 'TEXT':     (0xca, 0xd3, 0xf5),
        'SUBTEXT0': (0xa5, 0xad, 0xcb), 'LAVENDER': (0xb7, 0xbd, 0xf8),
        'BLUE':     (0x8a, 0xad, 0xf4), 'SAPPHIRE': (0x7d, 0xc4, 0xe4),
        'SKY':      (0x91, 0xd7, 0xe3), 'TEAL':     (0x8b, 0xd5, 0xca),
        'GREEN':    (0xa6, 0xda, 0x95), 'YELLOW':   (0xee, 0xd4, 0x9f),
        'MAUVE':    (0xc6, 0xa0, 0xf6), 'PINK':     (0xf5, 0xbd, 0xe6),
        'PEACH':    (0xf5, 0xa9, 0x7f), 'MAROON':   (0xee, 0x99, 0xa0),
    },
    'mocha': {
        'name': 'Mocha', 'light': False,
        'CRUST':    (0x11, 0x11, 0x1b), 'MANTLE':   (0x18, 0x18, 0x25),
        'BASE':     (0x1e, 0x1e, 0x2e), 'SURFACE0': (0x31, 0x32, 0x44),
        'SURFACE1': (0x45, 0x47, 0x5a), 'SURFACE2': (0x58, 0x5b, 0x70),
        'OVERLAY0': (0x6c, 0x70, 0x86), 'TEXT':     (0xcd, 0xd6, 0xf4),
        'SUBTEXT0': (0xa6, 0xad, 0xc8), 'LAVENDER': (0xb4, 0xbe, 0xfe),
        'BLUE':     (0x89, 0xb4, 0xfa), 'SAPPHIRE': (0x74, 0xc7, 0xec),
        'SKY':      (0x89, 0xdc, 0xeb), 'TEAL':     (0x94, 0xe2, 0xd5),
        'GREEN':    (0xa6, 0xe3, 0xa1), 'YELLOW':   (0xf9, 0xe2, 0xaf),
        'MAUVE':    (0xcb, 0xa6, 0xf7), 'PINK':     (0xf5, 0xc2, 0xe7),
        'PEACH':    (0xfa, 0xb3, 0x87), 'MAROON':   (0xeb, 0xa0, 0xac),
    },
}

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def lerp(a, b, t):
    return a + (b - a) * t

def lerp_color(c1, c2, t):
    return tuple(int(c1[i] + (c2[i] - c1[i]) * t) for i in range(3))

def clamp(v, lo, hi):
    return max(lo, min(hi, v))

# ---------------------------------------------------------------------------
# Scene components
# ---------------------------------------------------------------------------

class Stars:
    def __init__(self, count, width, height, p):
        self.stars = []
        for _ in range(count):
            color = random.choice([p['LAVENDER'], p['BLUE'], p['SKY'], p['TEXT'], p['SUBTEXT0']])
            self.stars.append({
                'x': random.randint(0, width), 'y': random.randint(0, int(height * 0.56)),
                'r': random.choice([1, 1, 1, 2]),
                'color': color,
                'phase': random.uniform(0, math.tau),
                'speed': random.uniform(0.4, 1.3),
            })
        self.t = 0.0

    def update(self, dt):
        self.t += dt

    def draw(self, surf):
        for s in self.stars:
            b = 0.5 + 0.5 * math.sin(self.t * s['speed'] + s['phase'])
            a = int(clamp(b * 255, 50, 255))
            r, g, b2 = s['color']
            tmp = pygame.Surface((s['r']*2+1, s['r']*2+1), pygame.SRCALPHA)
            pygame.draw.circle(tmp, (r, g, b2, a), (s['r'], s['r']), s['r'])
            surf.blit(tmp, (s['x'] - s['r'], s['y'] - s['r']))


class ShootingStar:
    def __init__(self, width, height, p):
        self.width, self.height, self.p = width, height, p
        self.active = False
        self.timer  = random.uniform(3, 10)

    def activate(self):
        angle = random.uniform(math.radians(205), math.radians(335))
        speed = random.uniform(350, 620)
        self.x = random.randint(int(self.width * 0.1), int(self.width * 0.9))
        self.y = random.randint(5, int(self.height * 0.28))
        self.vx, self.vy = math.cos(angle) * speed, math.sin(angle) * speed
        self.length = random.randint(70, 150)
        self.life   = 1.0
        self.active = True

    def update(self, dt):
        if not self.active:
            self.timer -= dt
            if self.timer <= 0:
                self.activate()
            return
        self.x += self.vx * dt
        self.y += self.vy * dt
        self.life -= dt * 1.9
        if self.life <= 0 or self.x < -200 or self.y > self.height * 0.55:
            self.active = False
            self.timer  = random.uniform(3, 10)

    def draw(self, surf):
        if not self.active:
            return
        spd = math.hypot(self.vx, self.vy)
        tx = self.x - self.vx / spd * self.length
        ty = self.y - self.vy / spd * self.length
        a  = int(clamp(self.life * 210, 0, 210))
        pygame.draw.line(surf, (*self.p['TEXT'], a), (int(tx), int(ty)), (int(self.x), int(self.y)), 1)
        pygame.draw.circle(surf, (*self.p['TEXT'], a), (int(self.x), int(self.y)), 1)


class Moon:
    def __init__(self, x, y, p):
        self.x, self.y, self.p = x, y, p
        self.t = 0.0

    def update(self, dt):
        self.t += dt * 0.14

    def draw(self, surf):
        p = self.p
        for r, a in [(72, 12), (58, 20), (48, 33), (40, 52)]:
            g = pygame.Surface((r*2, r*2), pygame.SRCALPHA)
            pulse = 0.85 + 0.15 * math.sin(self.t)
            pygame.draw.circle(g, (*p['LAVENDER'], int(a * pulse)), (r, r), r)
            surf.blit(g, (self.x - r, self.y - r), special_flags=pygame.BLEND_RGBA_ADD)
        pygame.draw.circle(surf, p['YELLOW'], (self.x, self.y), 28)
        sh = pygame.Surface((64, 64), pygame.SRCALPHA)
        pygame.draw.circle(sh, (*p['MANTLE'], 195), (38, 32), 26)
        surf.blit(sh, (self.x - 32 + 6, self.y - 32))
        for cx, cy, cr in [(self.x - 8, self.y + 5, 4), (self.x + 6, self.y - 8, 3)]:
            pygame.draw.circle(surf, lerp_color(p['YELLOW'], p['MANTLE'], 0.38), (cx, cy), cr)


class Aurora:
    def __init__(self, width, height, p):
        self.width, self.height = width, height
        self.bands = []
        for _ in range(4):
            self.bands.append({
                'y':     int(height * random.uniform(0.07, 0.37)),
                'color': random.choice([p['MAUVE'], p['TEAL'], p['BLUE'], p['GREEN'], p['LAVENDER']]),
                'phase': random.uniform(0, math.tau),
                'speed': random.uniform(0.17, 0.40),
                'amp':   random.uniform(16, 38),
                'freq':  random.uniform(0.003, 0.007),
                'alpha': random.randint(16, 44),
            })
        self.t = 0.0

    def update(self, dt):
        self.t += dt

    def draw(self, surf):
        for b in self.bands:
            pts_t, pts_b = [], []
            for x in range(0, self.width + 8, 8):
                off = b['amp'] * math.sin(b['freq'] * x + self.t * b['speed'] + b['phase'])
                pts_t.append((x, b['y'] + int(off) - 18))
                pts_b.append((x, b['y'] + int(off) + 18))
            s = pygame.Surface((self.width, self.height), pygame.SRCALPHA)
            pygame.draw.polygon(s, (*b['color'], b['alpha']), pts_t + list(reversed(pts_b)))
            surf.blit(s, (0, 0))


class Wave:
    def __init__(self, width, height, y_base, color, speed, amp, freq, alpha):
        self.width, self.height = width, height
        self.y_base = y_base
        self.color, self.speed, self.amp, self.freq, self.alpha = color, speed, amp, freq, alpha
        self.phase = random.uniform(0, math.tau)
        self.t = 0.0

    def update(self, dt, speed_mult=1.0):
        self.t += dt * speed_mult

    def get_y(self, x):
        return self.y_base + int(
            self.amp * math.sin(self.freq * x + self.t * self.speed + self.phase)
            + self.amp * 0.38 * math.sin(self.freq * 2.1 * x - self.t * self.speed * 0.72 + self.phase * 1.4)
        )

    def draw(self, surf):
        pts = [(0, self.height)]
        for x in range(0, self.width + 4, 4):
            pts.append((x, self.get_y(x)))
        pts.append((self.width, self.height))
        s = pygame.Surface((self.width, self.height), pygame.SRCALPHA)
        pygame.draw.polygon(s, (*self.color, self.alpha), pts)
        surf.blit(s, (0, 0))


class Foam:
    def __init__(self, width, height, wave, p):
        self.width, self.height, self.wave = width, height, wave
        self.p = p
        self.particles = [self._new(True) for _ in range(90)]

    def _new(self, init=False):
        x = random.randint(0, self.width)
        p = self.p
        return {
            'x': x, 'y': self.wave.get_y(x) if init else self.wave.get_y(x),
            'vy': -random.uniform(8, 28), 'vx': random.uniform(-10, 10),
            'size': random.uniform(1.2, 3.0),
            'life': random.uniform(0.4, 2.0), 'max_life': random.uniform(0.4, 2.0),
            'color': random.choice([p['SKY'], p['TEAL'], p['TEXT'], p['LAVENDER'], p['SAPPHIRE']]),
        }

    def update(self, dt):
        alive = []
        for p in self.particles:
            p['life'] -= dt
            if p['life'] > 0:
                p['y'] += p['vy'] * dt
                p['x'] += p['vx'] * dt
                alive.append(p)
            else:
                alive.append(self._new())
        self.particles = alive

    def draw(self, surf):
        for p in self.particles:
            a = int(clamp(p['life'] / p['max_life'] * 200, 0, 200))
            r = int(clamp(p['size'], 1, 4))
            s = pygame.Surface((r*2+2, r*2+2), pygame.SRCALPHA)
            pygame.draw.circle(s, (*p['color'], a), (r+1, r+1), r)
            surf.blit(s, (int(p['x']) - r - 1, int(p['y']) - r - 1))


class MoonReflection:
    def __init__(self, moon_x, width, horizon_y, bottom_y, p):
        self.moon_x, self.width, self.horizon_y, self.bottom_y, self.p = moon_x, width, horizon_y, bottom_y, p
        self.streaks = [{'x': moon_x + random.randint(-24, 24),
                         'phase': random.uniform(0, math.tau),
                         'speed': random.uniform(0.5, 1.5),
                         'w':     random.randint(2, 8)} for _ in range(14)]
        self.t = 0.0

    def update(self, dt):
        self.t += dt

    def draw(self, surf):
        for sk in self.streaks:
            a   = int(clamp(20 + 18 * math.sin(self.t * sk['speed'] + sk['phase']), 5, 55))
            xw  = sk['x'] + int(8 * math.sin(self.t * sk['speed'] * 0.7 + sk['phase']))
            for y in range(self.horizon_y, self.bottom_y, 6):
                tf = (y - self.horizon_y) / max(self.bottom_y - self.horizon_y, 1)
                w  = int(sk['w'] * (1 + tf * 2.5))
                a2 = int(a * (1 - tf * 0.45))
                xs = clamp(xw - w//2, 0, self.width)
                xe = clamp(xw + w//2, 0, self.width)
                s  = pygame.Surface((max(xe - xs, 1), 4), pygame.SRCALPHA)
                s.fill((*self.p['YELLOW'], a2))
                surf.blit(s, (xs, y))


class DistantLight:
    def __init__(self, x, y, p):
        self.x, self.y, self.p = x, y, p
        self.t = random.uniform(0, math.tau)

    def update(self, dt):
        self.t += dt * 1.1

    def draw(self, surf):
        b = 0.5 + 0.5 * math.sin(self.t)
        if b < 0.35:
            return
        a = int(clamp(b * 180, 0, 180))
        for r, fa in [(12, a//5), (7, a//3), (4, a)]:
            s = pygame.Surface((r*2, r*2), pygame.SRCALPHA)
            pygame.draw.circle(s, (*self.p['YELLOW'], fa), (r, r), r)
            surf.blit(s, (self.x - r, self.y - r))


class CatMascot:
    """The iconic Catppuccin cat floating and bobbing on the front wave."""
    def __init__(self, width, height, front_wave, size_name):
        self.width, self.height = width, height
        self.front_wave = front_wave

        size_map = {'small': 0.10, 'medium': 0.155, 'large': 0.22}
        px = int(height * size_map.get(size_name, 0.155))

        img = pygame.image.load(resource_path('assets/catppuccin_cat.png')).convert_alpha()
        self.image = pygame.transform.smoothscale(img, (px, px))
        self.px = px

        # Flip for water reflection
        refl = pygame.transform.flip(self.image, False, True)
        self.reflection = refl

        self.x           = float(width * 0.5)
        self.drift_phase = random.uniform(0, math.tau)
        self.bob_phase   = random.uniform(0, math.tau)
        self.t           = 0.0

    def update(self, dt):
        self.t += dt

    def draw(self, surf):
        # Drift slowly across the screen
        self.x = self.width * 0.5 + self.width * 0.28 * math.sin(self.t * 0.028 + self.drift_phase)
        wave_y = self.front_wave.get_y(int(self.x))
        bob    = int(7 * math.sin(self.t * 0.9 + self.bob_phase))
        ix     = int(self.x) - self.px // 2
        iy     = wave_y - self.px + bob

        # Faded reflection in the water
        ref = self.reflection.copy()
        ref.set_alpha(55)
        surf.blit(ref, (ix, wave_y + bob))

        # Cat logo
        surf.blit(self.image, (ix, iy))


# ---------------------------------------------------------------------------
# Sky / sea gradients
# ---------------------------------------------------------------------------

def draw_sky(surf, width, height, horizon_y, p):
    top = p['CRUST']
    mid = p['BASE']
    hor = lerp_color(p['SURFACE1'], p['SAPPHIRE'], 0.22)
    if p.get('light'):
        top = lerp_color(p['SKY'], (255, 255, 255), 0.55)
        mid = lerp_color(p['BASE'], p['SAPPHIRE'], 0.08)
        hor = lerp_color(p['SAPPHIRE'], p['TEAL'], 0.3)
    for y in range(horizon_y):
        t = y / horizon_y
        c = lerp_color(top, mid, t * 2) if t < 0.5 else lerp_color(mid, hor, (t - 0.5) * 2)
        pygame.draw.line(surf, c, (0, y), (width, y))


def draw_sea(surf, width, height, horizon_y, p):
    top = lerp_color(p['SAPPHIRE'], p['SURFACE1'], 0.28)
    bot = lerp_color(p['BASE'], p['CRUST'], 0.5)
    if p.get('light'):
        top = lerp_color(p['SKY'], p['TEAL'], 0.35)
        bot = lerp_color(p['SAPPHIRE'], p['SURFACE0'], 0.4)
    for y in range(horizon_y, height):
        t = (y - horizon_y) / max(height - horizon_y, 1)
        pygame.draw.line(surf, lerp_color(top, bot, t), (0, y), (width, y))


# ---------------------------------------------------------------------------
# Clock
# ---------------------------------------------------------------------------

def draw_clock(surf, fonts, width, height, p):
    now      = datetime.datetime.now()
    time_str = now.strftime("%H:%M")
    date_str = f"{now.strftime('%A, %B')} {now.day}"

    ts = fonts['large'].render(time_str, True, p['TEXT'])
    ds = fonts['small'].render(date_str, True, p['SUBTEXT0'])

    tx = width  - ts.get_width()  - 40
    ty = height - ts.get_height() - ds.get_height() - 22
    dx = width  - ds.get_width()  - 40
    dy = ty + ts.get_height() + 3

    # Shadow
    surf.blit(fonts['large'].render(time_str, True, p['CRUST']), (tx+2, ty+2))
    surf.blit(fonts['small'].render(date_str, True, p['CRUST']), (dx+2, dy+2))
    surf.blit(ts, (tx, ty))
    surf.blit(ds, (dx, dy))


# ---------------------------------------------------------------------------
# Main screensaver loop
# ---------------------------------------------------------------------------

def run_screensaver(preview_hwnd=None):
    settings   = load_settings()
    p          = PALETTES[settings.get('flavor', 'mocha')]
    speed_mult = float(settings.get('wave_speed', 1.0))

    pygame.init()
    pygame.mouse.set_visible(False)

    if preview_hwnd:
        os.environ["SDL_WINDOWID"] = str(preview_hwnd)
        rect = ctypes.wintypes.RECT()
        ctypes.windll.user32.GetClientRect(preview_hwnd, ctypes.byref(rect))
        W, H = rect.right or 320, rect.bottom or 240
        flags = 0
    else:
        info  = pygame.display.Info()
        W, H  = info.current_w, info.current_h
        flags = pygame.FULLSCREEN | pygame.NOFRAME

    screen = pygame.display.set_mode((W, H), flags)
    pygame.display.set_caption("Catppuccin Coast")

    horizon_y = int(H * 0.52)
    fp = pygame.font.match_font("segoeui,arial,sans")
    fonts = {
        'large': pygame.font.Font(fp, int(H * 0.072)),
        'small': pygame.font.Font(fp, int(H * 0.028)),
    }

    # Build scene
    stars    = Stars(220, W, H, p)
    shooting = [ShootingStar(W, H, p) for _ in range(3)]
    moon     = Moon(int(W * 0.18), int(H * 0.14), p)
    aurora   = Aurora(W, H, p)

    waves = [
        Wave(W, H, horizon_y + int(H*0.10), p['SAPPHIRE'], 0.35, 14, 0.012, 80),
        Wave(W, H, horizon_y + int(H*0.16), p['BLUE'],     0.50, 18, 0.010, 100),
        Wave(W, H, horizon_y + int(H*0.22), p['TEAL'],     0.65, 22, 0.009, 115),
        Wave(W, H, horizon_y + int(H*0.30), p['SAPPHIRE'], 0.80, 26, 0.008, 130),
        Wave(W, H, horizon_y + int(H*0.38), p['SKY'],      1.00, 30, 0.007, 145),
    ]
    front_wave = waves[-1]

    cat        = CatMascot(W, H, front_wave, settings.get('cat_size', 'medium'))
    foam       = Foam(W, H, front_wave, p)
    reflection = MoonReflection(int(W * 0.18), W, horizon_y, H, p)
    dist_lights = [DistantLight(int(W * x), horizon_y - 2, p) for x in (0.72, 0.81)]

    ticker    = pygame.time.Clock()
    prev_mouse = pygame.mouse.get_pos()
    mouse_frames = 0

    while True:
        dt = ticker.tick(60) / 1000.0

        for event in pygame.event.get():
            if event.type in (pygame.QUIT, pygame.KEYDOWN):
                pygame.quit(); return
            if event.type == pygame.MOUSEMOTION:
                cur = pygame.mouse.get_pos()
                if abs(cur[0]-prev_mouse[0]) + abs(cur[1]-prev_mouse[1]) > 6:
                    mouse_frames += 1
                    if mouse_frames > 3:
                        pygame.quit(); return
                prev_mouse = cur
            if event.type == pygame.MOUSEBUTTONDOWN:
                pygame.quit(); return

        draw_sky(screen, W, H, horizon_y, p)
        draw_sea(screen, W, H, horizon_y, p)

        if settings.get('show_aurora', True):
            aurora.update(dt)
            aurora.draw(screen)

        star_surf = pygame.Surface((W, H), pygame.SRCALPHA)
        stars.update(dt)
        stars.draw(star_surf)
        if settings.get('show_shooting', True):
            for ss in shooting:
                ss.update(dt)
                ss.draw(star_surf)
        screen.blit(star_surf, (0, 0))

        moon.update(dt)
        moon.draw(screen)

        for dl in dist_lights:
            dl.update(dt)
            dl.draw(screen)

        reflection.update(dt)
        reflection.draw(screen)

        for w in waves:
            w.update(dt, speed_mult)
            w.draw(screen)

        cat.update(dt)
        cat.draw(screen)

        if settings.get('show_foam', True):
            foam.update(dt)
            foam_surf = pygame.Surface((W, H), pygame.SRCALPHA)
            foam.draw(foam_surf)
            screen.blit(foam_surf, (0, 0))

        if settings.get('show_clock', True):
            draw_clock(screen, fonts, W, H, p)

        pygame.display.flip()


# ---------------------------------------------------------------------------
# Settings / config window
# ---------------------------------------------------------------------------

def show_config():
    pygame.init()
    W, H = 560, 500
    screen = pygame.display.set_mode((W, H))
    pygame.display.set_caption("Catppuccin Coast — Settings")

    # Always draw the config UI in Mocha for consistency
    p = PALETTES['mocha']

    fp         = pygame.font.match_font("segoeui,arial,sans")
    title_font = pygame.font.Font(fp, 22)
    label_font = pygame.font.Font(fp, 15)
    small_font = pygame.font.Font(fp, 13)

    cat_img = pygame.image.load(resource_path('assets/catppuccin_cat.png')).convert_alpha()
    cat_img = pygame.transform.smoothscale(cat_img, (72, 72))

    settings         = load_settings()
    selected_flavor  = settings.get('flavor', 'mocha')
    show_clock       = settings.get('show_clock', True)
    show_aurora      = settings.get('show_aurora', True)
    show_shooting    = settings.get('show_shooting', True)
    show_foam        = settings.get('show_foam', True)
    wave_speed_idx   = [0.5, 1.0, 1.8].index(
        min([0.5, 1.0, 1.8], key=lambda x: abs(x - float(settings.get('wave_speed', 1.0)))))
    cat_size_idx     = ['small', 'medium', 'large'].index(settings.get('cat_size', 'medium'))

    FLAVOR_ORDER = ['latte', 'frappe', 'macchiato', 'mocha']
    ticker = pygame.time.Clock()

    def draw_toggle(surf, rect, on, label):
        bg = p['SURFACE0'] if on else p['BASE']
        pygame.draw.rect(surf, bg, rect, border_radius=6)
        pygame.draw.rect(surf, p['SURFACE1'] if not on else p['TEAL'], rect, 1, border_radius=6)
        dot = p['TEAL'] if on else p['OVERLAY0']
        pygame.draw.circle(surf, dot, (rect.x + 15, rect.centery), 6)
        t = small_font.render(label, True, p['TEXT'] if on else p['SUBTEXT0'])
        surf.blit(t, (rect.x + 28, rect.centery - t.get_height()//2))

    def draw_choice_row(surf, x, y, w, options, selected_i, label):
        lbl = small_font.render(label, True, p['SUBTEXT0'])
        surf.blit(lbl, (x, y - 20))
        btn_w = w // len(options)
        rects = []
        for i, opt in enumerate(options):
            r = pygame.Rect(x + i * btn_w, y, btn_w - 4, 32)
            rects.append(r)
            active = i == selected_i
            bg   = p['SURFACE1'] if active else p['SURFACE0']
            bord = p['LAVENDER'] if active else p['SURFACE2']
            pygame.draw.rect(surf, bg, r, border_radius=5)
            pygame.draw.rect(surf, bord, r, 1 if not active else 2, border_radius=5)
            t = small_font.render(opt, True, p['TEXT'] if active else p['SUBTEXT0'])
            surf.blit(t, r.move(r.w//2 - t.get_width()//2, r.h//2 - t.get_height()//2))
        return rects

    flavor_rects  = {}
    toggle_rects  = {}
    speed_rects   = []
    cat_rects     = []
    save_rect     = pygame.Rect(W//2 - 90, H - 52, 180, 36)

    while True:
        mx, my = pygame.mouse.get_pos()
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit(); return
            if event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
                pygame.quit(); return
            if event.type == pygame.MOUSEBUTTONDOWN:
                for fk, r in flavor_rects.items():
                    if r.collidepoint(mx, my):
                        selected_flavor = fk
                for tk, r in toggle_rects.items():
                    if r.collidepoint(mx, my):
                        if   tk == 'clock':    show_clock    = not show_clock
                        elif tk == 'aurora':   show_aurora   = not show_aurora
                        elif tk == 'shooting': show_shooting = not show_shooting
                        elif tk == 'foam':     show_foam     = not show_foam
                for i, r in enumerate(speed_rects):
                    if r.collidepoint(mx, my):
                        wave_speed_idx = i
                for i, r in enumerate(cat_rects):
                    if r.collidepoint(mx, my):
                        cat_size_idx = i
                if save_rect.collidepoint(mx, my):
                    save_settings({
                        'flavor':         selected_flavor,
                        'show_clock':     show_clock,
                        'show_aurora':    show_aurora,
                        'show_shooting':  show_shooting,
                        'show_foam':      show_foam,
                        'wave_speed':     [0.5, 1.0, 1.8][wave_speed_idx],
                        'cat_size':       ['small', 'medium', 'large'][cat_size_idx],
                    })
                    pygame.quit(); return

        # --- Draw ---
        screen.fill(p['BASE'])

        # Header bar
        pygame.draw.rect(screen, p['SURFACE0'], (0, 0, W, 96))
        screen.blit(cat_img, (18, 12))
        screen.blit(title_font.render("Catppuccin Coast", True, p['LAVENDER']), (105, 18))
        screen.blit(small_font.render("Screensaver Settings", True, p['SUBTEXT0']), (107, 50))
        screen.blit(small_font.render(f"Current flavor: {PALETTES[selected_flavor]['name']}", True, p['TEAL']), (107, 68))

        # ---- Flavor row ----
        fy = 112
        screen.blit(label_font.render("FLAVOR", True, p['SUBTEXT0']), (20, fy))
        fy += 24
        flavor_rects = {}
        fw = (W - 40) // len(FLAVOR_ORDER)
        for i, fk in enumerate(FLAVOR_ORDER):
            fp2  = PALETTES[fk]
            rect = pygame.Rect(20 + i * fw, fy, fw - 6, 78)
            flavor_rects[fk] = rect
            active = fk == selected_flavor
            pygame.draw.rect(screen, fp2['BASE'], rect, border_radius=8)
            bw = 3 if active else 1
            pygame.draw.rect(screen, fp2['LAVENDER'] if active else fp2['SURFACE1'], rect, bw, border_radius=8)
            # Colour swatches
            swatches = [fp2['BLUE'], fp2['TEAL'], fp2['MAUVE'], fp2['PEACH'], fp2['GREEN']]
            for ci, sc in enumerate(swatches):
                pygame.draw.circle(screen, sc, (rect.x + 9 + ci * 19, rect.y + 20), 7)
            name_t = small_font.render(fp2['name'], True, fp2['TEXT'])
            screen.blit(name_t, (rect.x + 6, rect.y + 36))
            if fp2.get('light'):
                tag = small_font.render("light", True, fp2['SUBTEXT0'])
                screen.blit(tag, (rect.x + 6, rect.y + 54))

        # ---- Toggles ----
        ty2 = fy + 96
        screen.blit(label_font.render("FEATURES", True, p['SUBTEXT0']), (20, ty2))
        ty2 += 24
        toggle_rects = {}
        tog_items = [
            ('clock',    show_clock,    'Show clock'),
            ('aurora',   show_aurora,   'Aurora borealis'),
            ('shooting', show_shooting, 'Shooting stars'),
            ('foam',     show_foam,     'Seafoam particles'),
        ]
        for i, (tk, state, lbl) in enumerate(tog_items):
            col  = i % 2
            row  = i // 2
            rect = pygame.Rect(20 + col * 266, ty2 + row * 42, 256, 34)
            toggle_rects[tk] = rect
            draw_toggle(screen, rect, state, lbl)

        # ---- Wave speed ----
        wy = ty2 + 96
        speed_rects = draw_choice_row(screen, 20, wy, W - 40,
                                      ['Calm', 'Normal', 'Stormy'], wave_speed_idx, 'WAVE SPEED')

        # ---- Cat size ----
        cy = wy + 70
        cat_rects = draw_choice_row(screen, 20, cy, W - 40,
                                    ['Small', 'Medium', 'Large'], cat_size_idx, 'CAT SIZE')

        # ---- Save button ----
        hov = save_rect.collidepoint(mx, my)
        pygame.draw.rect(screen, p['BLUE'] if hov else p['LAVENDER'], save_rect, border_radius=8)
        sl = label_font.render("Save & Close", True, p['BASE'])
        screen.blit(sl, save_rect.move(save_rect.w//2 - sl.get_width()//2, save_rect.h//2 - sl.get_height()//2))

        pygame.display.flip()
        ticker.tick(30)


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

if __name__ == '__main__':
    args = [a.lower().strip() for a in sys.argv[1:]]
    if not args or '/s' in args:
        run_screensaver()
    elif '/c' in args:
        show_config()
    elif '/p' in args:
        idx = args.index('/p')
        try:
            hwnd = int(sys.argv[idx + 2])
        except (IndexError, ValueError):
            hwnd = None
        run_screensaver(preview_hwnd=hwnd)
    else:
        run_screensaver()
