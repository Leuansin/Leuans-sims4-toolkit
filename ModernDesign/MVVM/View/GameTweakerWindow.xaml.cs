using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LeuanS4ToolKit.Core;
using Microsoft.Win32;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class GameTweakerWindow : Window
    {
        private string _optionsIniPath;
        private Dictionary<string, string> _settings = new Dictionary<string, string>();
        private Dictionary<string, object> _controls = new Dictionary<string, object>();

        // Interesting settings to show with descriptions
        private readonly Dictionary<string, SettingInfo> _interestingSettings = new Dictionary<string, SettingInfo>
        {
            // Graphics
            { "simquality", new SettingInfo("Sim Quality (0-4)", "#8B5CF6",
                "Controls the detail level of Sims. 0=Low (simple textures), 1-2=Medium (moderate detail), 3-4=High/Ultra (maximum detail, smooth skin)",
                "Controla el nivel de detalle de los Sims. 0=Bajo (texturas simples), 1-2=Medio (detalle moderado), 3-4=Alto/Ultra (máximo detalle, piel suave)") },

            { "objectquality", new SettingInfo("Object Quality (0-4)", "#8B5CF6",
                "Detail level for furniture and objects. 0=Low (basic models), 2=Medium (good detail), 4=Ultra (highest quality models)",
                "Nivel de detalle para muebles y objetos. 0=Bajo (modelos básicos), 2=Medio (buen detalle), 4=Ultra (modelos de máxima calidad)") },

            { "lightingquality", new SettingInfo("Lighting Quality (0-4)", "#8B5CF6",
                "Quality of lighting and shadows. 0=No shadows, 2=Medium shadows, 4=Ultra (realistic shadows and lighting)",
                "Calidad de iluminación y sombras. 0=Sin sombras, 2=Sombras medias, 4=Ultra (sombras e iluminación realistas)") },

            { "terrainquality", new SettingInfo("Terrain Quality (0-4)", "#8B5CF6",
                "Detail of ground textures and terrain. 0=Low detail, 2=Medium, 4=High detail terrain",
                "Detalle de texturas del suelo y terreno. 0=Bajo detalle, 2=Medio, 4=Terreno de alto detalle") },

            { "generalreflections", new SettingInfo("Reflections (0-4)", "#8B5CF6",
                "Quality of reflections in mirrors, water, etc. 0=Off, 2=Medium, 4=Ultra realistic reflections",
                "Calidad de reflejos en espejos, agua, etc. 0=Desactivado, 2=Medio, 4=Reflejos ultra realistas") },

            { "viewdistance", new SettingInfo("View Distance (0-4)", "#8B5CF6",
                "How far you can see. 0=Very short, 2=Medium, 4=Maximum distance (may impact performance)",
                "Qué tan lejos puedes ver. 0=Muy corto, 2=Medio, 4=Distancia máxima (puede afectar rendimiento)") },

            { "edgesmoothing", new SettingInfo("Edge Smoothing (0-2)", "#8B5CF6",
                "Anti-aliasing to smooth jagged edges. 0=Off, 1=Medium, 2=High (smoother but slower)",
                "Anti-aliasing para suavizar bordes dentados. 0=Desactivado, 1=Medio, 2=Alto (más suave pero más lento)") },

            { "visualeffects", new SettingInfo("Visual Effects (0-4)", "#8B5CF6",
                "Quality of particle effects (fire, water, magic). 0=Minimal, 2=Medium, 4=Maximum effects",
                "Calidad de efectos de partículas (fuego, agua, magia). 0=Mínimo, 2=Medio, 4=Efectos máximos") },

            { "postprocessing", new SettingInfo("Post Processing (0/1)", "#8B5CF6",
                "Enables bloom, depth of field, and color grading. 0=Off (better FPS), 1=On (prettier)",
                "Activa bloom, profundidad de campo y gradación de color. 0=Desactivado (mejor FPS), 1=Activado (más bonito)") },

            { "useuncompressedtextures", new SettingInfo("Uncompressed Textures (0/1)", "#8B5CF6",
                "Uses high-quality textures. 0=Compressed (less VRAM), 1=Uncompressed (sharper but uses more VRAM)",
                "Usa texturas de alta calidad. 0=Comprimidas (menos VRAM), 1=Sin comprimir (más nítidas pero usa más VRAM)") },

            { "advancedrendering", new SettingInfo("Advanced Rendering (0/1)", "#8B5CF6",
                "Enables advanced rendering techniques. 0=Off, 1=On (better quality, slower)",
                "Activa técnicas de renderizado avanzadas. 0=Desactivado, 1=Activado (mejor calidad, más lento)") },

            { "visualquality", new SettingInfo("Visual Quality (0-4)", "#8B5CF6",
                "Overall visual quality preset. 0=Low, 1=Medium, 2=High, 3=Very High, 4=Ultra",
                "Preset de calidad visual general. 0=Bajo, 1=Medio, 2=Alto, 3=Muy Alto, 4=Ultra") },

            { "sceneresolution", new SettingInfo("Scene Resolution (0-4)", "#8B5CF6",
                "Internal rendering resolution. 0=Low (50%), 2=Medium (75%), 4=High (100%)",
                "Resolución de renderizado interno. 0=Bajo (50%), 2=Medio (75%), 4=Alto (100%)") },

            { "terrainslopescaling", new SettingInfo("Terrain Slope Scaling (0/1)", "#8B5CF6",
                "Enables detailed terrain slopes. 0=Off (flat), 1=On (realistic slopes)",
                "Activa pendientes de terreno detalladas. 0=Desactivado (plano), 1=Activado (pendientes realistas)") },
    
            // Performance
            { "frameratelimit", new SettingInfo("FPS Limit (30-200)", "#F59E0B",
                "Maximum frames per second. 30=Console-like, 60=Standard, 120+=High refresh rate monitors",
                "Máximo de fotogramas por segundo. 30=Tipo consola, 60=Estándar, 120+=Monitores de alta frecuencia") },

            { "verticalsync", new SettingInfo("V-Sync (0/1)", "#F59E0B",
                "Synchronizes with monitor refresh rate. 0=Off (may cause tearing), 1=On (prevents tearing, may add input lag)",
                "Sincroniza con la frecuencia del monitor. 0=Desactivado (puede causar tearing), 1=Activado (previene tearing, puede añadir lag)") },

            { "fullscreen", new SettingInfo("Fullscreen (0/1)", "#F59E0B",
                "Run in fullscreen mode. 0=Windowed, 1=Fullscreen (better performance)",
                "Ejecutar en pantalla completa. 0=Ventana, 1=Pantalla completa (mejor rendimiento)") },

            { "windowedfullscreen", new SettingInfo("Windowed Fullscreen (0/1)", "#F59E0B",
                "Borderless window mode. 0=Off, 1=On (easier alt-tabbing)",
                "Modo ventana sin bordes. 0=Desactivado, 1=Activado (más fácil cambiar de ventana)") },

            { "resolutionwidth", new SettingInfo("Resolution Width", "#F59E0B",
                "Horizontal resolution in pixels. Common: 1920, 2560, 3840",
                "Resolución horizontal en píxeles. Común: 1920, 2560, 3840") },

            { "resolutionheight", new SettingInfo("Resolution Height", "#F59E0B",
                "Vertical resolution in pixels. Common: 1080, 1440, 2160",
                "Resolución vertical en píxeles. Común: 1080, 1440, 2160") },

            { "resolutionrefresh", new SettingInfo("Refresh Rate (Hz)", "#F59E0B",
                "Monitor refresh rate. 0=Auto, 60=Standard, 120/144/240=High refresh",
                "Frecuencia de actualización del monitor. 0=Auto, 60=Estándar, 120/144/240=Alta frecuencia") },

            { "forcedx9", new SettingInfo("Force DirectX 9 (0/1)", "#F59E0B",
                "Forces older DirectX 9 mode. 0=Use best available, 1=Force DX9 (compatibility)",
                "Fuerza modo DirectX 9 antiguo. 0=Usar mejor disponible, 1=Forzar DX9 (compatibilidad)") },

            { "useosdpi", new SettingInfo("Use OS DPI (0/1)", "#F59E0B",
                "Use Windows DPI scaling. 0=Ignore, 1=Use OS scaling",
                "Usar escalado DPI de Windows. 0=Ignorar, 1=Usar escalado del SO") },

            { "usehidpi", new SettingInfo("Use Hi-DPI (0/1)", "#F59E0B",
                "Enable high DPI support. 0=Standard DPI, 1=Hi-DPI (4K monitors)",
                "Activar soporte Hi-DPI. 0=DPI estándar, 1=Hi-DPI (monitores 4K)") },

            { "disablescreensaverinfullscreenmode", new SettingInfo("Disable Screensaver in Fullscreen (0/1)", "#F59E0B",
                "Prevents screensaver when playing. 0=Allow screensaver, 1=Disable (recommended)",
                "Previene salvapantallas al jugar. 0=Permitir, 1=Desactivar (recomendado)") },
    
            // Gameplay
            { "autonomyhousehold", new SettingInfo("Household Autonomy (0-3)", "#10B981",
                "How much non-active Sims act on their own. 0=Off, 1=Low, 2=Medium, 3=Full autonomy",
                "Cuánto actúan por su cuenta los Sims no activos. 0=Desactivado, 1=Bajo, 2=Medio, 3=Autonomía completa") },

            { "autonomydisabledforactivesim", new SettingInfo("Disable Active Sim Autonomy (0/1)", "#10B981",
                "Prevents your controlled Sim from acting on their own. 0=Allow autonomy, 1=Full control",
                "Evita que tu Sim controlado actúe por su cuenta. 0=Permitir autonomía, 1=Control total") },

            { "whimsystem", new SettingInfo("Whim System (0/1)", "#10B981",
                "Enables random wants/whims. 0=Off (no whims), 1=On (Sims get random desires)",
                "Activa deseos/caprichos aleatorios. 0=Desactivado (sin caprichos), 1=Activado (Sims obtienen deseos aleatorios)") },

            { "lifestyleseffects", new SettingInfo("Lifestyles Effects (0/1)", "#10B981",
                "Enables lifestyle traits based on behavior. 0=Off, 1=On (Sims develop lifestyles)",
                "Activa rasgos de estilo de vida basados en comportamiento. 0=Desactivado, 1=Activado (Sims desarrollan estilos de vida)") },

            { "temperatureeffects", new SettingInfo("Temperature Effects (0/1)", "#10B981",
                "Sims react to hot/cold weather. 0=Off (no temperature reactions), 1=On (Sims feel temperature)",
                "Los Sims reaccionan al clima caliente/frío. 0=Desactivado (sin reacciones), 1=Activado (Sims sienten temperatura)") },

            { "ecofootprintgameplay", new SettingInfo("Eco Footprint (0/1)", "#10B981",
                "Enables eco footprint system. 0=Off, 1=On (neighborhoods have environmental states)",
                "Activa sistema de huella ecológica. 0=Desactivado, 1=Activado (vecindarios tienen estados ambientales)") },

            { "dustsystem", new SettingInfo("Dust System (0/1)", "#10B981",
                "Objects accumulate dust over time. 0=Off (no dust), 1=On (need to clean)",
                "Los objetos acumulan polvo con el tiempo. 0=Desactivado (sin polvo), 1=Activado (necesitas limpiar)") },

            { "moodchange", new SettingInfo("Mood Changes (0/1)", "#10B981",
                "Enables dynamic mood system. 0=Static moods, 1=Moods change based on events",
                "Activa sistema de estados de ánimo dinámicos. 0=Ánimos estáticos, 1=Ánimos cambian según eventos") },

            { "autoageunplayed", new SettingInfo("Auto-Age Unplayed Sims (0/1)", "#10B981",
                "Unplayed Sims age automatically. 0=Don't age, 1=Age with played Sims",
                "Sims no jugados envejecen automáticamente. 0=No envejecer, 1=Envejecer con Sims jugados") },

            { "simssetagingenabled", new SettingInfo("Sims Aging Enabled (0/1)", "#10B981",
                "Enables aging system. 0=No aging (immortal), 1=Sims age normally",
                "Activa sistema de envejecimiento. 0=Sin envejecimiento (inmortales), 1=Sims envejecen normalmente") },

            { "simssetagespeed", new SettingInfo("Aging Speed (0-3)", "#10B981",
                "How fast Sims age. 0=Short, 1=Normal, 2=Long, 3=Very Long",
                "Qué tan rápido envejecen los Sims. 0=Corto, 1=Normal, 2=Largo, 3=Muy Largo") },

            { "creatureaging", new SettingInfo("Pet Aging (0/1)", "#10B981",
                "Enables pet aging. 0=Pets don't age, 1=Pets age normally",
                "Activa envejecimiento de mascotas. 0=Mascotas no envejecen, 1=Mascotas envejecen normalmente") },

            { "seasonlength", new SettingInfo("Season Length (0-3)", "#10B981",
                "Duration of each season. 0=Short (7 days), 1=Normal (14 days), 2=Long (28 days)",
                "Duración de cada estación. 0=Corta (7 días), 1=Normal (14 días), 2=Larga (28 días)") },

            { "lunarcyclelength", new SettingInfo("Lunar Cycle Length (0-3)", "#10B981",
                "How long moon phases last. 0=Short, 1=Normal, 2=Long, 3=Very Long",
                "Cuánto duran las fases lunares. 0=Corto, 1=Normal, 2=Largo, 3=Muy Largo") },

            { "lunarphaselock", new SettingInfo("Lunar Phase Lock (0-8)", "#10B981",
                "Lock moon to specific phase. 0-7=Specific phase, 8=Normal cycle",
                "Bloquear luna en fase específica. 0-7=Fase específica, 8=Ciclo normal") },

            { "disablelunareffects", new SettingInfo("Disable Lunar Effects (0/1)", "#10B981",
                "Disables moon phase effects on Sims. 0=Effects enabled, 1=Effects disabled",
                "Desactiva efectos de fases lunares en Sims. 0=Efectos activados, 1=Efectos desactivados") },

            { "thundersnowstorms", new SettingInfo("Thunder/Snow Storms (0/1)", "#10B981",
                "Enables weather events. 0=Off (no storms), 1=On (weather variety)",
                "Activa eventos climáticos. 0=Desactivado (sin tormentas), 1=Activado (variedad climática)") },

            { "rainoptions", new SettingInfo("Rain Options (0-2)", "#10B981",
                "Rain frequency. 0=Less rain, 1=Normal, 2=More rain",
                "Frecuencia de lluvia. 0=Menos lluvia, 1=Normal, 2=Más lluvia") },

            { "snowoptions", new SettingInfo("Snow Options (0-2)", "#10B981",
                "Snow frequency. 0=Less snow, 1=Normal, 2=More snow",
                "Frecuencia de nieve. 0=Menos nieve, 1=Normal, 2=Más nieve") },

            { "icyconditions", new SettingInfo("Icy Conditions (0/1)", "#10B981",
                "Enables icy ground in winter. 0=Off, 1=On (Sims can slip)",
                "Activa suelo helado en invierno. 0=Desactivado, 1=Activado (Sims pueden resbalar)") },

            { "ailmentsenabled", new SettingInfo("Ailments Enabled (0/1)", "#10B981",
                "Sims can get sick. 0=Off (no illness), 1=On (Sims get sick)",
                "Los Sims pueden enfermarse. 0=Desactivado (sin enfermedades), 1=Activado (Sims se enferman)") },

            { "acne", new SettingInfo("Acne System (0/1)", "#10B981",
                "Teen Sims can get acne. 0=Off, 1=On (realistic teen skin)",
                "Sims adolescentes pueden tener acné. 0=Desactivado, 1=Activado (piel adolescente realista)") },

            { "careerlayoff", new SettingInfo("Career Layoff (0/1)", "#10B981",
                "Sims can be laid off from jobs. 0=Off (job security), 1=On (realistic)",
                "Los Sims pueden ser despedidos. 0=Desactivado (seguridad laboral), 1=Activado (realista)") },

            { "deathinventoryhandling", new SettingInfo("Death Inventory Handling (0-2)", "#10B981",
                "What happens to dead Sim's inventory. 0=Keep, 1=Transfer to family, 2=Delete",
                "Qué pasa con el inventario de Sims muertos. 0=Mantener, 1=Transferir a familia, 2=Eliminar") },

            { "npcreplacement", new SettingInfo("NPC Replacement (0/1)", "#10B981",
                "Replace dead NPCs with new ones. 0=Don't replace, 1=Auto-replace",
                "Reemplazar NPCs muertos con nuevos. 0=No reemplazar, 1=Auto-reemplazar") },

            { "maxprotectedsims", new SettingInfo("Max Protected Sims", "#10B981",
                "Number of Sims protected from culling. 0=None, higher=more protected",
                "Número de Sims protegidos de eliminación. 0=Ninguno, mayor=más protegidos") },

            { "selfdiscovery", new SettingInfo("Self Discovery (0/1)", "#10B981",
                "Enables self-discovery moments. 0=Off, 1=On (Sims discover preferences)",
                "Activa momentos de autodescubrimiento. 0=Desactivado, 1=Activado (Sims descubren preferencias)") },

            { "pivotalmomentsenabled", new SettingInfo("Pivotal Moments (0/1)", "#10B981",
                "Enables pivotal life moments. 0=Off, 1=On (important life events)",
                "Activa momentos vitales importantes. 0=Desactivado, 1=Activado (eventos importantes de vida)") },

            { "storyprogressioneffects", new SettingInfo("Story Progression (0/1)", "#10B981",
                "Enables story progression for unplayed Sims. 0=Off, 1=On (world evolves)",
                "Activa progresión de historia para Sims no jugados. 0=Desactivado, 1=Activado (el mundo evoluciona)") },

            { "npccivicvoting", new SettingInfo("NPC Civic Voting (0/1)", "#10B981",
                "NPCs vote on neighborhood actions. 0=Off, 1=On (democratic neighborhoods)",
                "NPCs votan en acciones del vecindario. 0=Desactivado, 1=Activado (vecindarios democráticos)") },

            { "restrictnpcfairies", new SettingInfo("Restrict NPC Fairies (0/1)", "#10B981",
                "Limits fairy NPCs spawning. 0=Allow all, 1=Restrict (fewer fairies)",
                "Limita aparición de NPCs hadas. 0=Permitir todos, 1=Restringir (menos hadas)") },

            { "restrictnpcwerewolves", new SettingInfo("Restrict NPC Werewolves (0/1)", "#10B981",
                "Limits werewolf NPCs spawning. 0=Allow all, 1=Restrict (fewer werewolves)",
                "Limita aparición de NPCs hombres lobo. 0=Permitir todos, 1=Restringir (menos hombres lobo)") },

            { "matchmakingoccultsimsenable", new SettingInfo("Matchmaking Occult Sims (0/1)", "#10B981",
                "Include occult Sims in matchmaking. 0=Off, 1=On (vampires, werewolves can match)",
                "Incluir Sims ocultos en emparejamiento. 0=Desactivado, 1=Activado (vampiros, hombres lobo pueden emparejar)") },

            { "matchmakinggallerysimsenable", new SettingInfo("Matchmaking Gallery Sims (0/1)", "#10B981",
                "Use Gallery Sims for matchmaking. 0=Off, 1=On (download Sims for dates)",
                "Usar Sims de Galería para emparejamiento. 0=Desactivado, 1=Activado (descargar Sims para citas)") },

            { "matchmakinggallerysimsfavoritesonlyenable", new SettingInfo("Matchmaking Gallery Favorites Only (0/1)", "#10B981",
                "Only use favorited Gallery Sims. 0=All Gallery, 1=Favorites only",
                "Solo usar Sims favoritos de Galería. 0=Toda la Galería, 1=Solo favoritos") },

            { "multiuniteventsenable", new SettingInfo("Multi-Unit Events (0/1)", "#10B981",
                "Enables apartment building events. 0=Off, 1=On (neighbor interactions)",
                "Activa eventos de edificios de apartamentos. 0=Desactivado, 1=Activado (interacciones con vecinos)") },

            { "multiunithiddenbydefault", new SettingInfo("Multi-Unit Hidden by Default (0/1)", "#10B981",
                "Hide other apartments by default. 0=Show all, 1=Hide (better performance)",
                "Ocultar otros apartamentos por defecto. 0=Mostrar todos, 1=Ocultar (mejor rendimiento)") },

            { "smallbusinesseventsenable", new SettingInfo("Small Business Events (0/1)", "#10B981",
                "Enables small business events. 0=Off, 1=On (business challenges)",
                "Activa eventos de pequeños negocios. 0=Desactivado, 1=Activado (desafíos de negocios)") },

            { "balancesystemenabled", new SettingInfo("Balance System (0/1)", "#10B981",
                "Enables game balance adjustments. 0=Off, 1=On (balanced gameplay)",
                "Activa ajustes de balance del juego. 0=Desactivado, 1=Activado (jugabilidad balanceada)") },

            { "buildecoeffects", new SettingInfo("Build Eco Effects (0/1)", "#10B981",
                "Building affects eco footprint. 0=Off, 1=On (eco-conscious building)",
                "Construcción afecta huella ecológica. 0=Desactivado, 1=Activado (construcción eco-consciente)") },

            { "famestartsimsoptedout", new SettingInfo("Fame Start Sims Opted Out (0/1)", "#10B981",
                "New Sims start opted out of fame. 0=Opt-in by default, 1=Opt-out by default",
                "Nuevos Sims empiezan sin fama. 0=Activada por defecto, 1=Desactivada por defecto") },
    
            // Camera & Controls
            { "advancedcamera", new SettingInfo("Advanced Camera (0/1)", "#38BDF8",
                "Unlocks advanced camera controls. 0=Standard camera, 1=Advanced (more freedom)",
                "Desbloquea controles avanzados de cámara. 0=Cámara estándar, 1=Avanzada (más libertad)") },

            { "cameraspeed", new SettingInfo("Camera Speed (0-200)", "#38BDF8",
                "How fast the camera moves. 50=Slow, 100=Normal, 150+=Fast",
                "Qué tan rápido se mueve la cámara. 50=Lento, 100=Normal, 150+=Rápido") },

            { "fpcameraspeed", new SettingInfo("First Person Camera Speed (0-200)", "#38BDF8",
                "Camera speed in first-person mode. 50=Slow, 100=Normal, 150+=Fast",
                "Velocidad de cámara en modo primera persona. 50=Lento, 100=Normal, 150+=Rápido") },

            { "fpdisablecamerabob", new SettingInfo("Disable First Person Camera Bob (0/1)", "#38BDF8",
                "Disables camera bobbing in first-person. 0=Bob enabled, 1=Disabled (stable view)",
                "Desactiva balanceo de cámara en primera persona. 0=Balanceo activado, 1=Desactivado (vista estable)") },

            { "fpinverthorizontalrotation", new SettingInfo("FP Invert Horizontal Rotation (0/1)", "#38BDF8",
                "Inverts horizontal camera in first-person. 0=Normal, 1=Inverted",
                "Invierte cámara horizontal en primera persona. 0=Normal, 1=Invertida") },

            { "fpinvertverticalrotation", new SettingInfo("FP Invert Vertical Rotation (0/1)", "#38BDF8",
                "Inverts vertical camera in first-person. 0=Normal, 1=Inverted",
                "Invierte cámara vertical en primera persona. 0=Normal, 1=Invertida") },

            { "edgescrolling", new SettingInfo("Edge Scrolling (0/1)", "#38BDF8",
                "Move camera by moving mouse to screen edges. 0=Off, 1=On",
                "Mover cámara al mover el mouse a los bordes. 0=Desactivado, 1=Activado") },

            { "edgescrollingwarning", new SettingInfo("Edge Scrolling Warning (0/1)", "#38BDF8",
                "Show warning about edge scrolling. 0=Don't show, 1=Show warning",
                "Mostrar advertencia sobre desplazamiento de bordes. 0=No mostrar, 1=Mostrar advertencia") },

            { "inverthorizontalrotation", new SettingInfo("Invert Horizontal Rotation (0/1)", "#38BDF8",
                "Inverts horizontal camera rotation. 0=Normal, 1=Inverted",
                "Invierte rotación horizontal de cámara. 0=Normal, 1=Invertida") },

            { "invertverticalrotation", new SettingInfo("Invert Vertical Rotation (0/1)", "#38BDF8",
                "Inverts vertical camera rotation. 0=Normal, 1=Inverted",
                "Invierte rotación vertical de cámara. 0=Normal, 1=Invertida") },

            { "cursorspeed", new SettingInfo("Cursor Speed (0-200)", "#38BDF8",
                "Mouse cursor movement speed. 50=Slow, 100=Normal, 150+=Fast",
                "Velocidad de movimiento del cursor. 50=Lento, 100=Normal, 150+=Rápido") },

            { "cursoracceleration", new SettingInfo("Cursor Acceleration (0-200)", "#38BDF8",
                "Cursor acceleration amount. 0=No acceleration, 100=Normal, 200=High",
                "Cantidad de aceleración del cursor. 0=Sin aceleración, 100=Normal, 200=Alta") },

            { "cursorscale", new SettingInfo("Cursor Scale (50-200)", "#38BDF8",
                "Size of mouse cursor. 50=Small, 100=Normal, 150+=Large",
                "Tamaño del cursor del mouse. 50=Pequeño, 100=Normal, 150+=Grande") },
    
            // Audio
            { "masterlevel", new SettingInfo("Master Volume (0-255)", "#EC4899",
                "Overall volume level. 0=Mute, 128=Half, 255=Maximum",
                "Nivel de volumen general. 0=Silencio, 128=Mitad, 255=Máximo") },

            { "musiclevel", new SettingInfo("Music Volume (0-255)", "#EC4899",
                "Background music volume. 0=Mute, 128=Half, 255=Maximum",
                "Volumen de música de fondo. 0=Silencio, 128=Mitad, 255=Máximo") },

            { "menumusiclevel", new SettingInfo("Menu Music Volume (0-255)", "#EC4899",
                "Menu music volume. 0=Mute, 128=Half, 255=Maximum",
                "Volumen de música de menú. 0=Silencio, 128=Mitad, 255=Máximo") },

            { "soundfxlevel", new SettingInfo("Sound FX Volume (0-255)", "#EC4899",
                "Sound effects volume. 0=Mute, 128=Half, 255=Maximum",
                "Volumen de efectos de sonido. 0=Silencio, 128=Mitad, 255=Máximo") },

            { "uisoundsxlevel", new SettingInfo("UI Sounds Volume (0-255)", "#EC4899",
                "User interface sounds volume. 0=Mute, 128=Half, 255=Maximum",
                "Volumen de sonidos de interfaz. 0=Silencio, 128=Mitad, 255=Máximo") },

            { "ambientlevel", new SettingInfo("Ambient Volume (0-255)", "#EC4899",
                "Background ambient sounds. 0=Mute, 128=Half, 255=Maximum",
                "Sonidos ambientales de fondo. 0=Silencio, 128=Mitad, 255=Máximo") },

            { "voicelevel", new SettingInfo("Voice Volume (0-255)", "#EC4899",
                "Simlish voice volume. 0=Mute, 128=Half, 255=Maximum",
                "Volumen de voces Simlish. 0=Silencio, 128=Mitad, 255=Máximo") },

            { "musicmute", new SettingInfo("Music Mute (0/1)", "#EC4899",
                "Mute all music. 0=Music on, 1=Music muted",
                "Silenciar toda la música. 0=Música activada, 1=Música silenciada") },

            { "soundfxmute", new SettingInfo("Sound FX Mute (0/1)", "#EC4899",
                "Mute sound effects. 0=SFX on, 1=SFX muted",
                "Silenciar efectos de sonido. 0=SFX activados, 1=SFX silenciados") },

            { "ambientmute", new SettingInfo("Ambient Mute (0/1)", "#EC4899",
                "Mute ambient sounds. 0=Ambient on, 1=Ambient muted",
                "Silenciar sonidos ambientales. 0=Ambiente activado, 1=Ambiente silenciado") },

            { "voicemute", new SettingInfo("Voice Mute (0/1)", "#EC4899",
                "Mute Simlish voices. 0=Voices on, 1=Voices muted",
                "Silenciar voces Simlish. 0=Voces activadas, 1=Voces silenciadas") },

            { "uimute", new SettingInfo("UI Mute (0/1)", "#EC4899",
                "Mute UI sounds. 0=UI sounds on, 1=UI muted",
                "Silenciar sonidos de interfaz. 0=Sonidos UI activados, 1=UI silenciada") },

            { "focusmute", new SettingInfo("Focus Mute (0/1)", "#EC4899",
                "Mute game when window loses focus. 0=Keep playing, 1=Mute when unfocused",
                "Silenciar juego cuando la ventana pierde foco. 0=Seguir sonando, 1=Silenciar sin foco") },

            { "audioquality", new SettingInfo("Audio Quality (0-3)", "#EC4899",
                "Audio processing quality. 0=Low, 1=Medium, 2=High, 3=Ultra",
                "Calidad de procesamiento de audio. 0=Bajo, 1=Medio, 2=Alto, 3=Ultra") },

            { "audiooutputmode", new SettingInfo("Audio Output Mode (0-2)", "#EC4899",
                "Speaker configuration. 0=Stereo, 1=Surround 5.1, 2=Surround 7.1",
                "Configuración de altavoces. 0=Estéreo, 1=Surround 5.1, 2=Surround 7.1") },

            { "matchspeedstereomusic", new SettingInfo("Match Speed Stereo Music (0/1)", "#EC4899",
                "Music speed matches game speed. 0=Normal speed, 1=Match game speed",
                "Velocidad de música coincide con velocidad del juego. 0=Velocidad normal, 1=Coincidir con juego") },

            { "safeforstreammusic", new SettingInfo("Safe for Stream Music (0/1)", "#EC4899",
                "Use copyright-safe music for streaming. 0=Normal music, 1=Stream-safe",
                "Usar música segura para streaming. 0=Música normal, 1=Segura para streaming") },

            { "videocapturesound", new SettingInfo("Video Capture Sound (0/1)", "#EC4899",
                "Include sound in video captures. 0=No sound, 1=Include sound",
                "Incluir sonido en capturas de video. 0=Sin sonido, 1=Incluir sonido") },
    
            // Mods & Custom Content
            { "scriptmodsenabled", new SettingInfo("Script Mods Enabled (0/1)", "#A855F7",
                "Allows script mods to run. 0=Disabled (safe), 1=Enabled (allows custom scripts)",
                "Permite ejecutar mods con scripts. 0=Desactivado (seguro), 1=Activado (permite scripts personalizados)") },

            { "modsdisabled", new SettingInfo("Mods Disabled (0/1)", "#A855F7",
                "Disables all mods and CC. 0=Mods enabled, 1=All mods disabled",
                "Desactiva todos los mods y CC. 0=Mods activados, 1=Todos los mods desactivados") },

            { "showmodliststartup", new SettingInfo("Show Mod List on Startup (0/1)", "#A855F7",
                "Shows mod list when game starts. 0=Don't show, 1=Show list",
                "Muestra lista de mods al iniciar. 0=No mostrar, 1=Mostrar lista") },
    
            // Online & Privacy
            { "enabletelemetry", new SettingInfo("Enable Telemetry (0/1)", "#06B6D4",
                "Sends usage data to EA. 0=Off (privacy), 1=On (helps developers)",
                "Envía datos de uso a EA. 0=Desactivado (privacidad), 1=Activado (ayuda a desarrolladores)") },

            { "showonlinenotifications", new SettingInfo("Online Notifications (0/1)", "#06B6D4",
                "Shows online notifications. 0=Off, 1=On",
                "Muestra notificaciones en línea. 0=Desactivado, 1=Activado") },

            { "onlineaccess", new SettingInfo("Online Access (0/1)", "#06B6D4",
                "Enable online features. 0=Offline mode, 1=Online (Gallery, updates)",
                "Activar funciones en línea. 0=Modo offline, 1=Online (Galería, actualizaciones)") },

            { "autoreconnect", new SettingInfo("Auto Reconnect (0/1)", "#06B6D4",
                "Automatically reconnect to online services. 0=Manual, 1=Auto-reconnect",
                "Reconectar automáticamente a servicios online. 0=Manual, 1=Auto-reconectar") },

            { "cdsautomaticupdates", new SettingInfo("Automatic Updates (0/1)", "#06B6D4",
                "Enable automatic game updates. 0=Manual updates, 1=Auto-update",
                "Activar actualizaciones automáticas del juego. 0=Actualizaciones manuales, 1=Auto-actualizar") },

            { "cdsautomaticupdatesunderage", new SettingInfo("Auto Updates Underage (0/1)", "#06B6D4",
                "Auto-updates for underage accounts. 0=Disabled, 1=Enabled",
                "Auto-actualizaciones para cuentas menores de edad. 0=Desactivado, 1=Activado") },

            { "cdspollfrequency", new SettingInfo("Update Check Frequency (0-5)", "#06B6D4",
                "How often to check for updates. 0=Never, 3=Normal, 5=Very often",
                "Frecuencia de verificación de actualizaciones. 0=Nunca, 3=Normal, 5=Muy seguido") },

            { "disablecomments", new SettingInfo("Disable Comments (0/1)", "#06B6D4",
                "Disable Gallery comments. 0=Show comments, 1=Hide comments",
                "Desactivar comentarios de Galería. 0=Mostrar comentarios, 1=Ocultar comentarios") },

            { "hidereportedcontent", new SettingInfo("Hide Reported Content (0/1)", "#06B6D4",
                "Hide content you've reported. 0=Show all, 1=Hide reported",
                "Ocultar contenido que has reportado. 0=Mostrar todo, 1=Ocultar reportado") },
    
            // UI & Interface
            { "uiscale", new SettingInfo("UI Scale (50-200)", "#F472B6",
                "Size of user interface. 100=Normal, 125=Larger, 150+=Very large",
                "Tamaño de interfaz de usuario. 100=Normal, 125=Más grande, 150+=Muy grande") },

            { "tradsocialmenuenabled", new SettingInfo("Traditional Social Menu (0/1)", "#F472B6",
                "Use old-style social menu. 0=Modern menu, 1=Traditional menu",
                "Usar menú social antiguo. 0=Menú moderno, 1=Menú tradicional") },

            { "screenshotpostui", new SettingInfo("Screenshot Post UI (0/1)", "#F472B6",
                "Include UI in screenshots. 0=Hide UI, 1=Show UI",
                "Incluir interfaz en capturas. 0=Ocultar UI, 1=Mostrar UI") },

            { "showpccheatsheet", new SettingInfo("Show PC Cheat Sheet (0/1)", "#F472B6",
                "Show keyboard shortcuts help. 0=Don't show, 1=Show cheat sheet",
                "Mostrar ayuda de atajos de teclado. 0=No mostrar, 1=Mostrar hoja de trucos") },
    
            // Tutorials & Guidance
            { "tutorialenabled", new SettingInfo("Tutorial Enabled (0/1)", "#34D399",
                "Enable in-game tutorials. 0=Off, 1=On (helpful for new players)",
                "Activar tutoriales en el juego. 0=Desactivado, 1=Activado (útil para nuevos jugadores)") },

            { "enableftuetutorialstart", new SettingInfo("Enable FTUE Tutorial Start (0/1)", "#34D399",
                "Enable first-time user experience. 0=Skip, 1=Show for new players",
                "Activar experiencia de primera vez. 0=Omitir, 1=Mostrar para nuevos jugadores") },

            { "guidanceenabled", new SettingInfo("Guidance Enabled (0/1)", "#34D399",
                "Enable gameplay guidance. 0=Off, 1=On (hints and tips)",
                "Activar guía de jugabilidad. 0=Desactivado, 1=Activado (pistas y consejos)") },

            { "guidanceautoenabled", new SettingInfo("Auto Guidance (0/1)", "#34D399",
                "Automatic guidance system. 0=Manual, 1=Auto (proactive tips)",
                "Sistema de guía automático. 0=Manual, 1=Auto (consejos proactivos)") },

            { "packguidanceenabled", new SettingInfo("Pack Guidance (0/1)", "#34D399",
                "Show guidance for expansion packs. 0=Off, 1=On (pack tutorials)",
                "Mostrar guía para packs de expansión. 0=Desactivado, 1=Activado (tutoriales de packs)") },

            { "guidancepack", new SettingInfo("Guidance Pack ID", "#34D399",
                "Which pack to show guidance for. Numeric pack ID",
                "Para qué pack mostrar guía. ID numérico de pack") },

            { "mutetutorialnarration", new SettingInfo("Mute Tutorial Narration (0/1)", "#34D399",
                "Mute tutorial voice-over. 0=Voice on, 1=Muted (text only)",
                "Silenciar narración de tutoriales. 0=Voz activada, 1=Silenciada (solo texto)") },

            { "memorieshelperenabled", new SettingInfo("Memories Helper (0/1)", "#34D399",
                "Enable memories system helper. 0=Off, 1=On (guides you through memories)",
                "Activar ayudante de sistema de recuerdos. 0=Desactivado, 1=Activado (te guía por recuerdos)") },
    
            // Video Capture
            { "videocapturequality", new SettingInfo("Video Capture Quality (0-3)", "#FB923C",
                "Quality of in-game video capture. 0=Low, 1=Medium, 2=High, 3=Ultra",
                "Calidad de captura de video en el juego. 0=Bajo, 1=Medio, 2=Alto, 3=Ultra") },

            { "videocapturesize", new SettingInfo("Video Capture Size (0-2)", "#FB923C",
                "Resolution of captured video. 0=Small, 1=Medium, 2=Large (1080p+)",
                "Resolución de video capturado. 0=Pequeño, 1=Medio, 2=Grande (1080p+)") },

            { "videocapturetime", new SettingInfo("Video Capture Time (0-3)", "#FB923C",
                "Length of video captures. 0=Short, 1=Medium, 2=Long, 3=Very long",
                "Duración de capturas de video. 0=Corto, 1=Medio, 2=Largo, 3=Muy largo") },

            { "videocapturehideui", new SettingInfo("Video Capture Hide UI (0/1)", "#FB923C",
                "Hide UI in video captures. 0=Show UI, 1=Hide UI (cinematic)",
                "Ocultar interfaz en capturas de video. 0=Mostrar UI, 1=Ocultar UI (cinemático)") },
    
            // Accessibility
            { "colorassisttype", new SettingInfo("Color Assist Type (0-3)", "#A78BFA",
                "Color blindness assistance mode. 0=Off, 1=Protanopia, 2=Deuteranopia, 3=Tritanopia",
                "Modo de asistencia para daltonismo. 0=Desactivado, 1=Protanopia, 2=Deuteranopia, 3=Tritanopia") },

            { "colorassistbrightness", new SettingInfo("Color Assist Brightness (0-4)", "#A78BFA",
                "Brightness of color assist. 0=Low, 2=Medium, 4=High",
                "Brillo de asistencia de color. 0=Bajo, 2=Medio, 4=Alto") },

            { "colorassistcontrast", new SettingInfo("Color Assist Contrast (0-4)", "#A78BFA",
                "Contrast of color assist. 0=Low, 2=Medium, 4=High",
                "Contraste de asistencia de color. 0=Bajo, 2=Medio, 4=Alto") },
    
            // Hardware & Devices
            { "lastdevice", new SettingInfo("Last Device", "#64748B",
                "Last used graphics device. Format: vendor;device;version",
                "Último dispositivo gráfico usado. Formato: fabricante;dispositivo;versión") },

            { "razeroptionenabled", new SettingInfo("Razer Integration (0/1)", "#64748B",
                "Enable Razer Chroma integration. 0=Off, 1=On (RGB lighting sync)",
                "Activar integración Razer Chroma. 0=Desactivado, 1=Activado (sincronización RGB)") },

            { "dynamickeyboardlightingenabled", new SettingInfo("Dynamic Keyboard Lighting (0/1)", "#64748B",
                "Enable dynamic RGB keyboard effects. 0=Off, 1=On (reactive lighting)",
                "Activar efectos RGB dinámicos de teclado. 0=Desactivado, 1=Activado (iluminación reactiva)") },
    
            // Miscellaneous
            { "luckenabled", new SettingInfo("Luck System (0/1)", "#FBBF24",
                "Enable luck system for Sims. 0=Off, 1=On (lucky/unlucky moments)",
                "Activar sistema de suerte para Sims. 0=Desactivado, 1=Activado (momentos afortunados/desafortunados)") },

            { "surveysenabled", new SettingInfo("Surveys Enabled (0/1)", "#FBBF24",
                "Allow in-game surveys. 0=No surveys, 1=Allow surveys",
                "Permitir encuestas en el juego. 0=Sin encuestas, 1=Permitir encuestas") },

            { "featurepreviewbuild", new SettingInfo("Feature Preview Build (0/1)", "#FBBF24",
                "Enable preview of upcoming features. 0=Stable only, 1=Preview features",
                "Activar vista previa de funciones próximas. 0=Solo estable, 1=Vista previa de funciones") },

            { "numboots", new SettingInfo("Number of Boots", "#FBBF24",
                "Times the game has been launched. Read-only counter",
                "Veces que se ha iniciado el juego. Contador de solo lectura") },

            { "animaticpack", new SettingInfo("Animatic Pack ID", "#FBBF24",
                "Currently loaded animatic pack. Numeric pack ID",
                "Pack animático cargado actualmente. ID numérico de pack") },
        };

        public GameTweakerWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            AutoDetectOptionsIni();
        }

        #region Language
        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            
            Title = "Game Tweaker";
            TitleText.Text = "⚙️ Game Tweaker";
            SubtitleText.Text = es ? "Editor avanzado de Options.ini" : "Advanced Options.ini editor";
            PathLabel.Text = "Options.ini:";
            BrowseButton.Content = es ? "📁 Buscar" : "📁 Browse";
            RestoreButton.Content = es ? "🔄 Restaurar" : "🔄 Restore";
            RestoreButton.ToolTip = es ? "Restaurar desde backup" : "Restore from backup";
            SaveButton.Content = es ? "💾 Guardar Cambios" : "💾 Save Changes";
            ReloadButton.Content = es ? "🔄 Recargar" : "🔄 Reload";
            CloseButton.Content = es ? "Cerrar" : "Close";
        }
        #endregion

        #region Auto-detect & Load
        private void AutoDetectOptionsIni()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            // Try "The Sims 4" first
            string path1 = Path.Combine(documentsPath, "Electronic Arts", "The Sims 4", "Options.ini");
            if (File.Exists(path1))
            {
                _optionsIniPath = path1;
                OptionsPathTextBox.Text = _optionsIniPath;
                CreateBackup();
                LoadSettings();
                return;
            }
            
            // Try "Los Sims 4"
            string path2 = Path.Combine(documentsPath, "Electronic Arts", "Los Sims 4", "Options.ini");
            if (File.Exists(path2))
            {
                _optionsIniPath = path2;
                OptionsPathTextBox.Text = _optionsIniPath;
                CreateBackup();
                LoadSettings();
                return;
            }

            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            MessageBox.Show(
                es ? "No se pudo detectar automáticamente Options.ini. Por favor, selecciona la ubicación manualmente." 
                   : "Could not auto-detect Options.ini. Please select the location manually.",
                es ? "Información" : "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CreateBackup()
        {
            if (string.IsNullOrEmpty(_optionsIniPath) || !File.Exists(_optionsIniPath))
                return;

            try
            {
                string backupPath = Path.Combine(Path.GetDirectoryName(_optionsIniPath), "Options.leubackup");
                
                // Only create backup if it doesn't exist yet
                if (!File.Exists(backupPath))
                {
                    File.Copy(_optionsIniPath, backupPath, true);
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error al crear backup: " : "Error creating backup: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void LoadSettings()
        {
            if (string.IsNullOrEmpty(_optionsIniPath) || !File.Exists(_optionsIniPath))
                return;

            try
            {
                _settings.Clear();
                var lines = File.ReadAllLines(_optionsIniPath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("[") || line.StartsWith(";"))
                        continue;

                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim().ToLower();
                        string value = parts[1].Trim();
                        _settings[key] = value;
                    }
                }

                BuildUI();
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error al cargar Options.ini: " : "Error loading Options.ini: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        #endregion

        #region UI Building
        private void BuildUI()
        {
            SettingsPanel.Children.Clear();
            _controls.Clear();

            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            // Group settings by category with colors
            var categories = new Dictionary<string, (List<string> settings, string color)>
            {
                { es ? "🎨 Gráficos" : "🎨 Graphics", (new List<string> {
                    "simquality", "objectquality", "lightingquality", "terrainquality",
                    "generalreflections", "viewdistance", "edgesmoothing", "visualeffects",
                    "postprocessing", "useuncompressedtextures", "advancedrendering",
                    "visualquality", "sceneresolution", "terrainslopescaling"
                }, "#8B5CF6") },

                { es ? "⚡ Rendimiento" : "⚡ Performance", (new List<string> {
                    "frameratelimit", "verticalsync", "fullscreen", "windowedfullscreen",
                    "resolutionwidth", "resolutionheight", "resolutionrefresh", "forcedx9",
                    "useosdpi", "usehidpi", "disablescreensaverinfullscreenmode"
                }, "#F59E0B") },

                { es ? "🎮 Gameplay" : "🎮 Gameplay", (new List<string> {
                    "autonomyhousehold", "autonomydisabledforactivesim", "whimsystem",
                    "lifestyleseffects", "temperatureeffects", "ecofootprintgameplay",
                    "dustsystem", "moodchange", "autoageunplayed", "simssetagingenabled",
                    "simssetagespeed", "creatureaging", "seasonlength", "lunarcyclelength",
                    "lunarphaselock", "disablelunareffects", "thundersnowstorms", "rainoptions",
                    "snowoptions", "icyconditions", "ailmentsenabled", "acne", "careerlayoff",
                    "deathinventoryhandling", "npcreplacement", "maxprotectedsims", "selfdiscovery",
                    "pivotalmomentsenabled", "storyprogressioneffects", "npccivicvoting",
                    "restrictnpcfairies", "restrictnpcwerewolves", "matchmakingoccultsimsenable",
                    "matchmakinggallerysimsenable", "matchmakinggallerysimsfavoritesonlyenable",
                    "multiuniteventsenable", "multiunithiddenbydefault", "smallbusinesseventsenable",
                    "balancesystemenabled", "buildecoeffects", "famestartsimsoptedout"
                }, "#10B981") },

                { es ? "📷 Cámara y Controles" : "📷 Camera & Controls", (new List<string> {
                    "advancedcamera", "cameraspeed", "fpcameraspeed", "fpdisablecamerabob",
                    "fpinverthorizontalrotation", "fpinvertverticalrotation", "edgescrolling",
                    "edgescrollingwarning", "inverthorizontalrotation", "invertverticalrotation",
                    "cursorspeed", "cursoracceleration", "cursorscale"
                }, "#38BDF8") },

                { es ? "🔊 Audio" : "🔊 Audio", (new List<string> {
                    "masterlevel", "musiclevel", "menumusiclevel", "soundfxlevel",
                    "uisoundsxlevel", "ambientlevel", "voicelevel", "musicmute",
                    "soundfxmute", "ambientmute", "voicemute", "uimute", "focusmute",
                    "audioquality", "audiooutputmode", "matchspeedstereomusic",
                    "safeforstreammusic", "videocapturesound"
                }, "#EC4899") },

                { es ? "🔧 Mods y Contenido Personalizado" : "🔧 Mods & Custom Content", (new List<string> {
                    "scriptmodsenabled", "modsdisabled", "showmodliststartup"
                }, "#A855F7") },

                { es ? "🌐 Online y Privacidad" : "🌐 Online & Privacy", (new List<string> {
                    "enabletelemetry", "showonlinenotifications", "onlineaccess", "autoreconnect",
                    "cdsautomaticupdates", "cdsautomaticupdatesunderage", "cdspollfrequency",
                    "disablecomments", "hidereportedcontent"
                }, "#06B6D4") },

                { es ? "💻 Interfaz de Usuario" : "💻 UI & Interface", (new List<string> {
                    "uiscale", "tradsocialmenuenabled", "screenshotpostui", "showpccheatsheet"
                }, "#F472B6") },

                { es ? "📚 Tutoriales y Guía" : "📚 Tutorials & Guidance", (new List<string> {
                    "tutorialenabled", "enableftuetutorialstart", "guidanceenabled",
                    "guidanceautoenabled", "packguidanceenabled", "guidancepack",
                    "mutetutorialnarration", "memorieshelperenabled"
                }, "#34D399") },

                { es ? "🎥 Captura de Video" : "🎥 Video Capture", (new List<string> {
                    "videocapturequality", "videocapturesize", "videocapturetime", "videocapturehideui"
                }, "#FB923C") },

                { es ? "♿ Accesibilidad" : "♿ Accessibility", (new List<string> {
                    "colorassisttype", "colorassistbrightness", "colorassistcontrast"
                }, "#A78BFA") },

                { es ? "🖥️ Hardware y Dispositivos" : "🖥️ Hardware & Devices", (new List<string> {
                    "lastdevice", "razeroptionenabled", "dynamickeyboardlightingenabled"
                }, "#64748B") },

                { es ? "⭐ Misceláneos" : "⭐ Miscellaneous", (new List<string> {
                    "luckenabled", "surveysenabled", "featurepreviewbuild", "numboots", "animaticpack"
                }, "#FBBF24") }
            };

            foreach (var category in categories)
            {
                // Category header with color
                var categoryTitle = new TextBlock
                {
                    Text = category.Key,
                    Style = (Style)FindResource("TitleStyle"),
                    FontSize = 16,
                    Margin = new Thickness(0, 15, 0, 10),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(category.Value.color))
                };
                SettingsPanel.Children.Add(categoryTitle);

                // Settings in this category
                foreach (var settingKey in category.Value.settings)
                {
                    if (_settings.ContainsKey(settingKey))
                    {
                        AddSettingControl(settingKey, _settings[settingKey]);
                    }
                }
            }
        }

        private void AddSettingControl(string key, string value)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            
            var border = new Border
            {
                Style = (Style)FindResource("SettingCard")
            };

            // Add tooltip with description
            if (_interestingSettings.ContainsKey(key))
            {
                var info = _interestingSettings[key];
                border.ToolTip = es ? info.DescriptionES : info.DescriptionEN;
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });

            // Label with color
            var label = new TextBlock
            {
                Text = _interestingSettings.ContainsKey(key) ? _interestingSettings[key].DisplayName : key,
                Style = (Style)FindResource("BodyStyle"),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            
            // Apply category color to label
            if (_interestingSettings.ContainsKey(key))
            {
                label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_interestingSettings[key].CategoryColor));
            }
            
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            // Determine control type based on value
            UIElement inputControl = null;

            if (int.TryParse(value, out int intValue))
            {
                // Check if it's a boolean (0/1)
                if ((intValue == 0 || intValue == 1) && !key.Contains("level") && !key.Contains("speed") && !key.Contains("limit") && !key.Contains("acceleration"))
                {
                    var checkBox = new CheckBox
                    {
                        IsChecked = intValue == 1,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"))
                    };
                    inputControl = checkBox;
                }
                else
                {
                    // Numeric slider
                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    
                    int maxValue = 4;
                    if (key.Contains("level")) maxValue = 255;
                    else if (key.Contains("speed") || key.Contains("acceleration")) maxValue = 200;
                    else if (key == "frameratelimit") maxValue = 200;
                    else if (key == "edgesmoothing") maxValue = 2;

                    var slider = new Slider
                    {
                        Minimum = 0,
                        Maximum = maxValue,
                        Value = intValue,
                        Width = 120,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 10, 0)
                    };

                    var valueLabel = new TextBlock
                    {
                        Text = intValue.ToString(),
                        Style = (Style)FindResource("BodyStyle"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 40,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
                    };

                    slider.ValueChanged += (s, e) => valueLabel.Text = ((int)slider.Value).ToString();

                    stackPanel.Children.Add(slider);
                    stackPanel.Children.Add(valueLabel);
                    inputControl = stackPanel;
                }
            }
            else
            {
                // Text input
                var textBox = new TextBox
                {
                    Text = value,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A")),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#374151")),
                    Padding = new Thickness(8),
                    FontFamily = new FontFamily("Bahnschrift Light")
                };
                inputControl = textBox;
            }

            Grid.SetColumn(inputControl, 1);
            grid.Children.Add(inputControl);

            border.Child = grid;
            SettingsPanel.Children.Add(border);

            _controls[key] = inputControl;
        }
        #endregion

        #region Save & Reload
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            
            // Show warning message
            string warningTitle = es ? "⚠️ Confirmar Cambios" : "⚠️ Confirm Changes";
            string warningMessage = es
                ? "¿Estás seguro de que quieres aplicar estos cambios?\n\n" +
                  "⚠️ ADVERTENCIA: Modificar estas configuraciones sin conocimiento puede corromper el archivo Options.ini.\n\n" +
                  "✅ No te preocupes: Si algo sale mal, siempre puedes presionar el botón 'Restaurar' para volver al estado original.\n\n" +
                  "¿Deseas continuar?"
                : "Are you sure you want to apply these changes?\n\n" +
                  "⚠️ WARNING: Modifying these settings without knowledge may corrupt the Options.ini file.\n\n" +
                  "✅ Don't worry: If something goes wrong, you can always press the 'Restore' button to return to the original state.\n\n" +
                  "Do you want to continue?";

            var confirmResult = MessageBox.Show(
                warningMessage,
                warningTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult != MessageBoxResult.Yes)
                return;

            if (string.IsNullOrEmpty(_optionsIniPath) || !File.Exists(_optionsIniPath))
            {
                MessageBox.Show(
                    es ? "No se ha seleccionado un archivo Options.ini válido." : "No valid Options.ini file selected.",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Read all lines
                var lines = File.ReadAllLines(_optionsIniPath).ToList();

                // Update values
                foreach (var kvp in _controls)
                {
                    string key = kvp.Key;
                    object control = kvp.Value;
                    string newValue = GetControlValue(control);

                    // Find and update the line
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Trim().ToLower().StartsWith(key + " =") || lines[i].Trim().ToLower().StartsWith(key + "="))
                        {
                            lines[i] = $"{key} = {newValue}";
                            break;
                        }
                    }
                }

                // Write back
                File.WriteAllLines(_optionsIniPath, lines);

                MessageBox.Show(
                    es ? "¡Cambios guardados exitosamente! Reinicia el juego para aplicar los cambios." 
                       : "Changes saved successfully! Restart the game to apply changes.",
                    es ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{(es ? "Error al guardar: " : "Error saving: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GetControlValue(object control)
        {
            if (control is CheckBox checkBox)
            {
                return checkBox.IsChecked == true ? "1" : "0";
            }
            else if (control is StackPanel stackPanel)
            {
                var slider = stackPanel.Children.OfType<Slider>().FirstOrDefault();
                if (slider != null)
                {
                    return ((int)slider.Value).ToString();
                }
            }
            else if (control is TextBox textBox)
            {
                return textBox.Text;
            }

            return "0";
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }
        #endregion

        #region Restore
        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            
            if (string.IsNullOrEmpty(_optionsIniPath))
            {
                MessageBox.Show(
                    es ? "No se ha seleccionado un archivo Options.ini." : "No Options.ini file selected.",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string backupPath = Path.Combine(Path.GetDirectoryName(_optionsIniPath), "Options.leubackup");

            if (!File.Exists(backupPath))
            {
                MessageBox.Show(
                    es ? "No se encontró el archivo de backup (Options.leubackup)." : "Backup file not found (Options.leubackup).",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                es ? "¿Estás seguro de que quieres restaurar desde el backup?\n\nEsto eliminará todos los cambios actuales y restaurará la configuración original."
                   : "Are you sure you want to restore from backup?\n\nThis will delete all current changes and restore the original configuration.",
                es ? "Confirmar Restauración" : "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Delete current Options.ini
                    if (File.Exists(_optionsIniPath))
                    {
                        File.Delete(_optionsIniPath);
                    }

                    // Rename backup to Options.ini
                    File.Copy(backupPath, _optionsIniPath, true);

                    MessageBox.Show(
                        es ? "¡Restauración exitosa! Se ha restaurado la configuración original." 
                           : "Restore successful! Original configuration has been restored.",
                        es ? "Éxito" : "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Reload settings
                    LoadSettings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"{(es ? "Error al restaurar: " : "Error restoring: ")}{ex.Message}",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region Browse
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*",
                Title = "Select Options.ini"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _optionsIniPath = openFileDialog.FileName;
                OptionsPathTextBox.Text = _optionsIniPath;
                CreateBackup();
                LoadSettings();
            }
        }
        #endregion

        #region Close
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Helper Class
        private class SettingInfo
        {
            public string DisplayName { get; set; }
            public string CategoryColor { get; set; }
            public string DescriptionEN { get; set; }
            public string DescriptionES { get; set; }

            public SettingInfo(string displayName, string categoryColor, string descriptionEN, string descriptionES)
            {
                DisplayName = displayName;
                CategoryColor = categoryColor;
                DescriptionEN = descriptionEN;
                DescriptionES = descriptionES;
            }
        }
        #endregion
    }
}