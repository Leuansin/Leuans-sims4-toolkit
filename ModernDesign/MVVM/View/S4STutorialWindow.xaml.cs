using ModernDesign.Localization;
using ModernDesign.Profile;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class S4STutorialWindow : Window
    {
        private string _category;
        private int _currentStep = 0;
        private List<TutorialStep> _steps;
        private string _tutorialId;
        private bool _isShowingMedal = false; // Medallas

        public S4STutorialWindow(string category)
        {
            InitializeComponent();
            _category = category;
            _tutorialId = $"tutorial_{category}";
            LoadSteps();
            ApplyLanguage();
            UpdateUI();
        }

        public class TutorialStep
        {
            public string Title { get; set; }
            public string Desc { get; set; }
            public string Image { get; set; }
            public string Label { get; set; }
            public string Color { get; set; }
            public string Tips { get; set; }
        }

        private void LoadSteps()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            _steps = GetStepsForCategory(_category, es);
        }

        private List<TutorialStep> GetStepsForCategory(string cat, bool es)
        {
            switch (cat)
            {
                case "trait":
                    return GetTraitSteps(es);
                case "interaction":
                    return GetInteractionSteps(es);
                case "career":
                    return GetCareerSteps(es);
                case "object":
                    return GetObjectSteps(es);
                case "clothing":
                    return GetClothingSteps(es);
                case "buff":
                    return GetBuffSteps(es);
                default:
                    return new List<TutorialStep>();
            }
        }

        // ===== TRAIT TUTORIAL =====
        private List<TutorialStep> GetTraitSteps(bool es)
        {
            if (es)
            {
                return new List<TutorialStep>
        {
            new TutorialStep
            {
                Title = "Paso 1: Descargar e Instalar Mod Constructor V5",
                Desc = "Ve a GitHub (github.com/Zerbu/Mod-Constructor-5/releases) y descarga la última versión (v5.0-beta.11 o superior). Descomprime el archivo ZIP en una carpeta. Ejecuta 'Constructor.exe'. Si te pide Python, ve a python.org, descarga Python 3.7+ y marca 'Add Python to PATH' durante la instalación.",
                Image = "/MVVM/View/logo.png",
                Label = "Descargar",
                Color = "#22C55E",
                Tips = "Python es necesario para que los script mods funcionen. Sin Python, algunos rasgos no funcionarán correctamente."
            },
            new TutorialStep
            {
                Title = "Paso 2: Crear Nuevo Mod y Agregar Trait",
                Desc = "En Mod Constructor, haz clic en 'New Mod'. Ingresa tu nombre de creador cuando te lo pida (ej: 'TuNombre'). Luego haz clic en el botón 'Add Element' (esquina inferior izquierda) → selecciona '1: Beginner' → 'Trait'. Dale un nombre único sin espacios como 'Gamer_Trait' o 'Creative_Soul'. Haz clic en 'Create Element'.",
                Image = "/MVVM/View/logo.png",
                Label = "Crear Trait",
                Color = "#22C55E",
                Tips = "Usa nombres descriptivos y únicos con guiones bajos (_) en lugar de espacios para evitar conflictos con otros mods."
            },
            new TutorialStep
            {
                Title = "Paso 3: Configurar Información Básica del Trait",
                Desc = "En la pestaña 'Trait Info': 1) Ingresa el 'Display Name' (nombre que se verá en el juego, ej: 'Pro Gamer'). 2) Escribe una 'Description' atractiva. 3) Haz clic en 'Select Icon' para elegir un ícono del juego o importar uno personalizado (64x64 PNG). 4) Marca las casillas de edad: Teen, Young Adult, Adult, Elder según corresponda. 5) Deja 'Trait Type' en 'Personality' si es un rasgo normal de CAS.",
                Image = "/MVVM/View/logo.png",
                Label = "Info Básica",
                Color = "#38BDF8",
                Tips = "Los íconos personalizados deben ser 64x64 píxeles en formato PNG con fondo transparente."
            },
            new TutorialStep
            {
                Title = "Paso 4: Crear Core Buff (Emoción Base)",
                Desc = "Expande la sección 'Core Buff' y haz clic en 'Open Element'. Esto abre el buff principal del rasgo. En la pestaña del buff: 1) Ve a 'Add Component' → 'Emotion'. 2) Selecciona el tipo de emoción (Happy, Confident, Focused, Playful, Flirty, Sad, Angry, etc.). 3) Ajusta el 'Value' (intensidad) entre +1 y +10 (recomendado: +2 o +3 para balance).",
                Image = "/MVVM/View/logo.png",
                Label = "Core Buff",
                Color = "#EAB308",
                Tips = "Un valor de +2 o +3 es equilibrado. Valores muy altos (+8 o más) dominarán todas las demás emociones del Sim."
            },
            new TutorialStep
            {
                Title = "Paso 5: Agregar Modificadores de Habilidades (Opcional)",
                Desc = "Vuelve a la pestaña del Trait. Haz clic en 'Add Component' → 'Modifiers'. En la ventana que se abre: 1) Ve a 'Add Component' → 'Skill Rate Modifier'. 2) Selecciona una habilidad (ej: Video Gaming, Programming, Charisma). 3) Ajusta el multiplicador (1.5 = +50% más rápido, 0.5 = 50% más lento). Puedes agregar múltiples modificadores de diferentes habilidades.",
                Image = "/MVVM/View/logo.png",
                Label = "Modificadores",
                Color = "#A78BFA",
                Tips = "Los modificadores de habilidades hacen que tu rasgo sea más funcional. Un valor de 1.25 (+25%) a 1.5 (+50%) es razonable."
            },
            new TutorialStep
            {
                Title = "Paso 6: Añadir Whims/Wants (Deseos)",
                Desc = "En la pestaña del Trait, ve a 'Add Component' → 'Whims/Wants'. Haz clic en el botón '+' para agregar deseos. Puedes: 1) Seleccionar whims del juego usando 'Game Reference'. 2) Crear whims personalizados. Los whims hacen que tus Sims con este rasgo quieran realizar ciertas acciones frecuentemente (ej: jugar videojuegos, socializar, trabajar).",
                Image = "/MVVM/View/logo.png",
                Label = "Whims",
                Color = "#EC4899",
                Tips = "Los whims hacen que el rasgo se sienta más vivo. Los Sims actuarán más acorde a su personalidad."
            },
            new TutorialStep
            {
                Title = "Paso 7: Configurar Trait Conflicts (Conflictos)",
                Desc = "En la pestaña 'Trait Conflicts': marca los rasgos del juego que sean incompatibles con el tuyo. Por ejemplo, si tu rasgo es 'Antisocial', deberías marcar 'Outgoing' como conflicto. Esto evita que un Sim tenga rasgos contradictorios al mismo tiempo. Busca rasgos por nombre o navega por las categorías.",
                Image = "/MVVM/View/logo.png",
                Label = "Conflictos",
                Color = "#F97316",
                Tips = "Piensa lógicamente: ¿qué rasgos serían opuestos al tuyo? Esto hace que tus Sims sean más coherentes."
            },
            new TutorialStep
            {
                Title = "Paso 8: Agregar Autonomía Personalizada (Avanzado)",
                Desc = "Para hacer que tu rasgo influya en el comportamiento autónomo, ve al Core Buff → 'Add Component' → 'Commodities'. Selecciona commodities de otros rasgos (ej: 'trait_Active' para que hagan ejercicio, 'trait_Neat' para limpiar). Esto hace que tus Sims actúen de forma similar a los Sims con esos rasgos base.",
                Image = "/MVVM/View/logo.png",
                Label = "Autonomía",
                Color = "#38BDF8",
                Tips = "Las commodities controlan qué acciones los Sims prefieren hacer por sí solos. Experimenta con diferentes combinaciones."
            },
            new TutorialStep
            {
                Title = "Paso 9: Agregar Loots/Events Ocasionales (Avanzado)",
                Desc = "Para eventos especiales, ve a 'Add Component' → 'Occasional Events' o 'Continuous Loots'. Occasional Events ocurren aleatoriamente (ej: ganar dinero, recibir objetos). Continuous Loots se evalúan constantemente bajo ciertas condiciones. Puedes agregar modificadores de necesidades, cambios de relaciones, o recompensas monetarias.",
                Image = "/MVVM/View/logo.png",
                Label = "Loots",
                Color = "#22C55E",
                Tips = "Usa esto para hacer rasgos más interesantes, como un rasgo 'Afortunado' que ocasionalmente da dinero."
            },
            new TutorialStep
            {
                Title = "Paso 10: Exportar e Instalar el Mod",
                Desc = "Haz clic en 'Export Package' (esquina inferior derecha). Elige dónde guardar y dale un nombre al archivo. Se generarán dos archivos: un .package y un .ts4script. Copia AMBOS archivos a tu carpeta Mods (Documentos/Electronic Arts/The Sims 4/Mods). Asegúrate de tener mods y script mods activados en las opciones del juego.",
                Image = "/MVVM/View/logo.png",
                Label = "Exportar",
                Color = "#22C55E",
                Tips = "Siempre copia ambos archivos (.package y .ts4script). Sin el .ts4script, muchas funciones no funcionarán."
            },
            new TutorialStep
            {
                Title = "Paso 11: Probar en el Juego",
                Desc = "Abre Los Sims 4. Ve a Crear un Sim (CAS). Habilita los trucos con Ctrl+Shift+C y escribe 'testingcheats true', luego 'cas.fulleditmode'. Busca tu rasgo personalizado en la sección de rasgos. También puedes usar el comando 'traits.equip_trait [NombreDelRasgo]' para asignarlo a un Sim activo rápidamente.",
                Image = "/MVVM/View/logo.png",
                Label = "¡Probar!",
                Color = "#22C55E",
                Tips = "Si tu rasgo no aparece, verifica que ambos archivos estén en Mods y que los mods estén activados en Opciones del Juego."
            }
        };
            }
            else // English
            {
                return new List<TutorialStep>
        {
            new TutorialStep
            {
                Title = "Step 1: Download and Install Mod Constructor V5",
                Desc = "Go to GitHub (github.com/Zerbu/Mod-Constructor-5/releases) and download the latest version (v5.0-beta.11 or higher). Unzip the file to a folder. Run 'Constructor.exe'. If it asks for Python, go to python.org, download Python 3.7+ and check 'Add Python to PATH' during installation.",
                Image = "/MVVM/View/logo.png",
                Label = "Download",
                Color = "#22C55E",
                Tips = "Python is required for script mods to work. Without Python, some traits won't function properly."
            },
            new TutorialStep
            {
                Title = "Step 2: Create New Mod and Add Trait",
                Desc = "In Mod Constructor, click 'New Mod'. Enter your creator name when prompted (e.g., 'YourName'). Then click the 'Add Element' button (bottom left corner) → select '1: Beginner' → 'Trait'. Give it a unique name without spaces like 'Gamer_Trait' or 'Creative_Soul'. Click 'Create Element'.",
                Image = "/MVVM/View/logo.png",
                Label = "Create Trait",
                Color = "#22C55E",
                Tips = "Use descriptive, unique names with underscores (_) instead of spaces to avoid conflicts with other mods."
            },
            new TutorialStep
            {
                Title = "Step 3: Configure Basic Trait Information",
                Desc = "In the 'Trait Info' tab: 1) Enter the 'Display Name' (name shown in-game, e.g., 'Pro Gamer'). 2) Write an engaging 'Description'. 3) Click 'Select Icon' to choose a game icon or import a custom one (64x64 PNG). 4) Check the age boxes: Teen, Young Adult, Adult, Elder as appropriate. 5) Leave 'Trait Type' as 'Personality' for regular CAS traits.",
                Image = "/MVVM/View/logo.png",
                Label = "Basic Info",
                Color = "#38BDF8",
                Tips = "Custom icons should be 64x64 pixels in PNG format with transparent background."
            },
            new TutorialStep
            {
                Title = "Step 4: Create Core Buff (Base Emotion)",
                Desc = "Expand the 'Core Buff' section and click 'Open Element'. This opens the trait's main buff. In the buff tab: 1) Go to 'Add Component' → 'Emotion'. 2) Select emotion type (Happy, Confident, Focused, Playful, Flirty, Sad, Angry, etc.). 3) Adjust the 'Value' (intensity) between +1 and +10 (recommended: +2 or +3 for balance).",
                Image = "/MVVM/View/logo.png",
                Label = "Core Buff",
                Color = "#EAB308",
                Tips = "A value of +2 or +3 is balanced. Very high values (+8 or more) will dominate all other Sim emotions."
            },
            new TutorialStep
            {
                Title = "Step 5: Add Skill Modifiers (Optional)",
                Desc = "Return to the Trait tab. Click 'Add Component' → 'Modifiers'. In the window that opens: 1) Go to 'Add Component' → 'Skill Rate Modifier'. 2) Select a skill (e.g., Video Gaming, Programming, Charisma). 3) Adjust the multiplier (1.5 = +50% faster, 0.5 = 50% slower). You can add multiple modifiers for different skills.",
                Image = "/MVVM/View/logo.png",
                Label = "Modifiers",
                Color = "#A78BFA",
                Tips = "Skill modifiers make your trait more functional. A value of 1.25 (+25%) to 1.5 (+50%) is reasonable."
            },
            new TutorialStep
            {
                Title = "Step 6: Add Whims/Wants",
                Desc = "In the Trait tab, go to 'Add Component' → 'Whims/Wants'. Click the '+' button to add wants. You can: 1) Select game whims using 'Game Reference'. 2) Create custom whims. Whims make Sims with this trait want to perform certain actions frequently (e.g., play video games, socialize, work).",
                Image = "/MVVM/View/logo.png",
                Label = "Whims",
                Color = "#EC4899",
                Tips = "Whims make the trait feel more alive. Sims will act more according to their personality."
            },
            new TutorialStep
            {
                Title = "Step 7: Configure Trait Conflicts",
                Desc = "In the 'Trait Conflicts' tab: check game traits that are incompatible with yours. For example, if your trait is 'Antisocial', you should mark 'Outgoing' as a conflict. This prevents a Sim from having contradictory traits at the same time. Search traits by name or browse categories.",
                Image = "/MVVM/View/logo.png",
                Label = "Conflicts",
                Color = "#F97316",
                Tips = "Think logically: what traits would be opposite to yours? This makes your Sims more coherent."
            },
            new TutorialStep
            {
                Title = "Step 8: Add Custom Autonomy (Advanced)",
                Desc = "To make your trait influence autonomous behavior, go to Core Buff → 'Add Component' → 'Commodities'. Select commodities from other traits (e.g., 'trait_Active' for exercising, 'trait_Neat' for cleaning). This makes your Sims act similarly to Sims with those base traits.",
                Image = "/MVVM/View/logo.png",
                Label = "Autonomy",
                Color = "#38BDF8",
                Tips = "Commodities control which actions Sims prefer to do on their own. Experiment with different combinations."
            },
            new TutorialStep
            {
                Title = "Step 9: Add Occasional Loots/Events (Advanced)",
                Desc = "For special events, go to 'Add Component' → 'Occasional Events' or 'Continuous Loots'. Occasional Events happen randomly (e.g., gain money, receive objects). Continuous Loots are evaluated constantly under certain conditions. You can add need modifiers, relationship changes, or monetary rewards.",
                Image = "/MVVM/View/logo.png",
                Label = "Loots",
                Color = "#22C55E",
                Tips = "Use this to make more interesting traits, like a 'Lucky' trait that occasionally gives money."
            },
            new TutorialStep
            {
                Title = "Step 10: Export and Install the Mod",
                Desc = "Click 'Export Package' (bottom right corner). Choose where to save and name your file. Two files will be generated: a .package and a .ts4script. Copy BOTH files to your Mods folder (Documents/Electronic Arts/The Sims 4/Mods). Make sure mods and script mods are enabled in game options.",
                Image = "/MVVM/View/logo.png",
                Label = "Export",
                Color = "#22C55E",
                Tips = "Always copy both files (.package and .ts4script). Without the .ts4script, many functions won't work."
            },
            new TutorialStep
            {
                Title = "Step 11: Test In-Game",
                Desc = "Open The Sims 4. Go to Create a Sim (CAS). Enable cheats with Ctrl+Shift+C and type 'testingcheats true', then 'cas.fulleditmode'. Look for your custom trait in the traits section. You can also use the command 'traits.equip_trait [TraitName]' to quickly assign it to an active Sim.",
                Image = "/MVVM/View/logo.png",
                Label = "Test!",
                Color = "#22C55E",
                Tips = "If your trait doesn't appear, verify both files are in Mods and mods are enabled in Game Options."
            }
        };
            }
        }

        // ===== INTERACTION TUTORIAL =====
        private List<TutorialStep> GetInteractionSteps(bool es)
        {
            return new List<TutorialStep> {
        new TutorialStep { Title = es?"Paso 1: Abrir Mod Constructor":"Step 1: Open Mod Constructor", Desc = es?"Abre Mod Constructor V5 y crea un nuevo mod o abre uno existente.":"Open Mod Constructor V5 and create a new mod or open existing one.", Image="/MVVM/View/logo.png", Label=es?"Inicio":"Start", Color="#EC4899" },
        new TutorialStep {
            Title = es ? "Paso 2: Agregar Interacción Social" : "Step 2: Add Social Interaction",
            Desc = es
                ? "1. Selecciona 'Add Element'\n2. Busca 'Social Interaction' (SuperInteraction es una interacción de nivel INTERMEDIO, por lo que no lo abarcaremos aca)\n3. Ponle el nombre que llevará esta interacción (el nombre que aparecerá en el menú)\n4. Presiona 'Create Element'"
                : "1. Select 'Add Element'\n2. Search for 'Social Interaction' (SuperInteraction is a INTERMEDIATE level interaction, so we will not address that here)\n3. Give the interaction the name that will appear in the menu\n4. Press 'Create Element'",
            Image = "//MVVM/View/logo.png",
            Label = es ? "Crear" : "Create",
            Color = "#EC4899"
        },
        new TutorialStep { Title = es?"Paso 3: Configurar Categoría y Nombre":"Step 3: Configure Category and Name", Desc = es?"Selecciona la categoría de menú (Friendly, Mean, Romance, Funny, Mischief). Ingresa el nombre visible y descripción de la interacción.":"Select menu category (Friendly, Mean, Romance, Funny, Mischief). Enter visible name and interaction description.", Image="/MVVM/View/logo.png", Label=es?"Config":"Config", Color="#A78BFA" },
        new TutorialStep { Title = es?"Paso 4: Seleccionar Animación":"Step 4: Select Animation", Desc = es?"En 'Animation', elige una plantilla predefinida (Talk, Hug, High Five, Tell Story, etc.). Esto define la animación visual.":"In 'Animation', choose a predefined template (Talk, Hug, High Five, Tell Story, etc.). This defines the visual animation.", Image="/MVVM/View/logo.png", Label=es?"Animar":"Animate", Color="#38BDF8" },
        new TutorialStep { Title = es?"Paso 5: Configurar Resultados":"Step 5: Configure Outcomes", Desc = es?"Define qué sucede al ejecutarse: agrega buffs, cambia relaciones, da dinero. Puedes tener múltiples resultados.":"Define what happens when executed: add buffs, change relationships, give money. You can have multiple outcomes.", Image="/MVVM/View/logo.png", Label=es?"Efectos":"Effects", Color="#22C55E" },
        new TutorialStep { Title = es?"Paso 6: Exportar y Probar":"Step 6: Export and Test", Desc = es?"Exporta el mod (.package y .ts4script). Colócalos en tu carpeta Mods y prueba haciendo clic en otro Sim en el juego.":"Export the mod (.package and .ts4script). Place them in your Mods folder and test by clicking on another Sim in-game.", Image="/MVVM/View/logo.png", Label=es?"¡Listo!":"Done!", Color="#22C55E" }
    };
        }

        // ===== CAREER TUTORIAL =====
        private List<TutorialStep> GetCareerSteps(bool es)
        {
            return new List<TutorialStep> {
        new TutorialStep { Title = es?"Paso 1: Agregar Career Element":"Step 1: Add Career Element", Desc = es?"En Mod Constructor, ve a 'Add Element' → '2: Intermediate' → 'Career'. Dale un nombre único.":"In Mod Constructor, go to 'Add Element' → '2: Intermediate' → 'Career'. Give it a unique name.", Image="/MVVM/View/logo.png", Label=es?"Crear":"Create", Color="#F97316" },
        new TutorialStep { Title = es?"Paso 2: Configurar Info de la Carrera":"Step 2: Configure Career Info", Desc = es?"Ingresa nombre visible, descripción detallada y selecciona un ícono. Elige si es Full-Time o Part-Time.":"Enter visible name, detailed description and select an icon. Choose if it's Full-Time or Part-Time.", Image="/MVVM/View/logo.png", Label=es?"Info":"Info", Color="#F97316" },
        new TutorialStep { Title = es?"Paso 3: Crear Career Track y Niveles":"Step 3: Create Career Track and Levels", Desc = es?"Abre 'Start Track'. En 'Career Levels', agrega niveles (típicamente 10). Para cada nivel: nombre del puesto, salario/hora, días laborales, horario.":"Open 'Start Track'. In 'Career Levels', add levels (typically 10). For each level: job title, hourly pay, work days, schedule.", Image="/MVVM/View/logo.png", Label=es?"Niveles":"Levels", Color="#22C55E" },
        new TutorialStep { Title = es?"Paso 4: Agregar Tareas de Promoción":"Step 4: Add Promotion Tasks", Desc = es?"Para cada nivel, agrega objetivos que el Sim debe cumplir para ascender (ej: alcanzar nivel 5 de habilidad, hacer amigos).":"For each level, add objectives the Sim must complete to advance (e.g., reach skill level 5, make friends).", Image="/MVVM/View/logo.png", Label=es?"Tareas":"Tasks", Color="#38BDF8" },
        new TutorialStep { Title = es?"Paso 5: Agregar Recompensas por Nivel":"Step 5: Add Level Rewards", Desc = es?"Opcionalmente, agrega objetos de recompensa por nivel. Usa Sims 4 Studio para buscar IDs de objetos del juego.":"Optionally, add reward objects per level. Use Sims 4 Studio to find object IDs from the game.", Image="/MVVM/View/logo.png", Label=es?"Premios":"Rewards", Color="#EAB308" },
        new TutorialStep { Title = es?"Paso 6: Exportar y Probar":"Step 6: Export and Test", Desc = es?"Exporta el mod. En el juego, haz que un Sim busque trabajo en su teléfono. Tu carrera debería aparecer. Usa 'careers.promote [CareerName]' para probar rápido.":"Export the mod. In-game, have a Sim search for jobs on their phone. Your career should appear. Use 'careers.promote [CareerName]' to test quickly.", Image="/MVVM/View/logo.png", Label=es?"¡Listo!":"Done!", Color="#22C55E" }
    };
        }

        // ===== OBJECT TUTORIAL =====
        private List<TutorialStep> GetObjectSteps(bool es)
        {
            return new List<TutorialStep> {
        new TutorialStep { Title = es?"Paso 1: Clonar Objeto Base en S4S":"Step 1: Clone Base Object in S4S", Desc = es?"Abre Sims 4 Studio. Ve a 'Build' → 'Object'. Busca un objeto similar al que quieres crear y clónalo.":"Open Sims 4 Studio. Go to 'Build' → 'Object'. Find a similar object to what you want to create and clone it.", Image="/MVVM/View/logo.png", Label=es?"Clonar":"Clone", Color="#38BDF8" },
        new TutorialStep { Title = es?"Paso 2: Exportar y Editar Texturas":"Step 2: Export and Edit Textures", Desc = es?"En S4S, exporta las texturas del objeto. Edítalas en Photoshop, GIMP o Paint.NET. Cambia colores, patrones o diseños.":"In S4S, export the object textures. Edit them in Photoshop, GIMP or Paint.NET. Change colors, patterns or designs.", Image="/MVVM/View/logo.png", Label=es?"Texturas":"Textures", Color="#A78BFA" },
        new TutorialStep { Title = es?"Paso 3: Importar Texturas Editadas":"Step 3: Import Edited Textures", Desc = es?"Guarda tus texturas editadas y vuelve a S4S. Importa las nuevas texturas usando el botón 'Import'.":"Save your edited textures and return to S4S. Import the new textures using the 'Import' button.", Image="/MVVM/View/logo.png", Label=es?"Importar":"Import", Color="#A78BFA" },
        new TutorialStep { Title = es?"Paso 4: Editar Mesh en Blender (Avanzado)":"Step 4: Edit Mesh in Blender (Advanced)", Desc = es?"Si necesitas cambiar la forma 3D: exporta el mesh a Blender, edita la geometría y re-importa. Esto es opcional y avanzado.":"If you need to change 3D shape: export mesh to Blender, edit geometry and re-import. This is optional and advanced.", Image="/MVVM/View/logo.png", Label="Blender", Color="#EA580C", Tips=es?"Para principiantes, solo cambia texturas. Meshes requieren conocimientos de modelado 3D.":"For beginners, just change textures. Meshes require 3D modeling knowledge." },
        new TutorialStep { Title = es?"Paso 5: Configurar Catálogo":"Step 5: Configure Catalog", Desc = es?"En S4S, configura el nombre, descripción, precio y categoría de catálogo donde aparecerá tu objeto en Modo Construcción.":"In S4S, configure name, description, price and catalog category where your object will appear in Build Mode.", Image="/MVVM/View/logo.png", Label=es?"Catálogo":"Catalog", Color="#22C55E" },
        new TutorialStep { Title = es?"Paso 6: Guardar y Probar":"Step 6: Save and Test", Desc = es?"Guarda como .package con nombre único. Colócalo en Mods. Busca tu objeto en Modo Construcción en la categoría que elegiste.":"Save as .package with unique name. Place in Mods. Find your object in Build Mode in the category you chose.", Image="/MVVM/View/logo.png", Label=es?"¡Listo!":"Done!", Color="#22C55E" }
    };
        }

        // ===== CLOTHING TUTORIAL =====
        private List<TutorialStep> GetClothingSteps(bool es)
        {
            return new List<TutorialStep> {
        new TutorialStep { Title = es?"Paso 1: Clonar Ropa Base":"Step 1: Clone Base Clothing", Desc = es?"Abre Sims 4 Studio. Ve a 'CAS' → selecciona categoría (Top, Bottom, Dress, etc.). Busca una prenda similar y clónala.":"Open Sims 4 Studio. Go to 'CAS' → select category (Top, Bottom, Dress, etc.). Find similar clothing and clone it.", Image="/MVVM/View/logo.png", Label=es?"Clonar":"Clone", Color="#EC4899" },
        new TutorialStep { Title = es?"Paso 2: Exportar Texturas":"Step 2: Export Textures", Desc = es?"Exporta las texturas de la prenda. Verás múltiples archivos (difusa, normal, specular). La difusa es la más importante para recolorear.":"Export clothing textures. You'll see multiple files (diffuse, normal, specular). Diffuse is most important for recoloring.", Image="/MVVM/View/logo.png", Label=es?"Exportar":"Export", Color="#A78BFA" },
        new TutorialStep { Title = es?"Paso 3: Editar en Editor de Imágenes":"Step 3: Edit in Image Editor", Desc = es?"Abre las texturas en Photoshop/GIMP. Cambia colores, agrega patrones o diseños. Mantén el mismo tamaño de imagen.":"Open textures in Photoshop/GIMP. Change colors, add patterns or designs. Keep the same image size.", Image="/MVVM/View/logo.png", Label=es?"Editar":"Edit", Color="#38BDF8" },
        new TutorialStep { Title = es?"Paso 4: Importar Texturas Editadas":"Step 4: Import Edited Textures", Desc = es?"Vuelve a S4S e importa tus texturas editadas. Asegúrate de importar en los slots correctos (difusa, normal, etc.).":"Return to S4S and import your edited textures. Make sure to import in correct slots (diffuse, normal, etc.).", Image="/MVVM/View/logo.png", Label=es?"Importar":"Import", Color="#22C55E" },
        new TutorialStep { Title = es?"Paso 5: Configurar Categorías CAS":"Step 5: Configure CAS Categories", Desc = es?"Configura en qué categorías aparecerá (Everyday, Formal, Athletic, etc.). Marca las edades permitidas y género.":"Configure which categories it will appear in (Everyday, Formal, Athletic, etc.). Check allowed ages and gender.", Image="/MVVM/View/logo.png", Label=es?"Categorías":"Categories", Color="#EAB308" },
        new TutorialStep { Title = es?"Paso 6: Guardar y Probar":"Step 6: Save and Test", Desc = es?"Guarda el .package. Colócalo en Mods. Abre CAS en el juego y busca tu prenda en la categoría correspondiente.":"Save the .package. Place in Mods. Open CAS in-game and find your clothing in the corresponding category.", Image="/MVVM/View/logo.png", Label=es?"¡Listo!":"Done!", Color="#22C55E" }
    };
        }

        // ===== BUFF TUTORIAL =====
        private List<TutorialStep> GetBuffSteps(bool es)
        {
            return new List<TutorialStep> {
        new TutorialStep { Title = es?"Paso 1: Crear Buff en Mod Constructor":"Step 1: Create Buff in Mod Constructor", Desc = es?"Abre Mod Constructor. Ve a 'Add Element' → '1: Beginner' → 'Buff'. Dale un nombre único.":"Open Mod Constructor. Go to 'Add Element' → '1: Beginner' → 'Buff'. Give it a unique name.", Image="/MVVM/View/logo.png", Label=es?"Crear":"Create", Color="#3B82F6" },
        new TutorialStep { Title = es?"Paso 2: Configurar Info del Buff":"Step 2: Configure Buff Info", Desc = es?"Ingresa nombre visible, descripción y selecciona un ícono. Define la duración (en horas de juego) o márcalo como permanente.":"Enter visible name, description and select icon. Define duration (in game hours) or mark as permanent.", Image="/MVVM/View/logo.png", Label=es?"Info":"Info", Color="#3B82F6" },
        new TutorialStep { Title = es?"Paso 3: Agregar Emoción":"Step 3: Add Emotion", Desc = es?"Ve a 'Add Component' → 'Emotion'. Selecciona el tipo (Happy, Sad, Angry, etc.) y ajusta la intensidad (+1 a +10).":"Go to 'Add Component' → 'Emotion'. Select type (Happy, Sad, Angry, etc.) and adjust intensity (+1 to +10).", Image="/MVVM/View/logo.png", Label=es?"Emoción":"Emotion", Color="#22C55E" },
        new TutorialStep { Title = es?"Paso 4: Agregar Modificadores (Opcional)":"Step 4: Add Modifiers (Optional)", Desc = es?"Agrega modificadores de habilidades, necesidades o autonomía. Esto hace que el buff afecte el comportamiento del Sim.":"Add skill, need or autonomy modifiers. This makes the buff affect Sim behavior.", Image="/MVVM/View/logo.png", Label=es?"Modificadores":"Modifiers", Color="#A78BFA" },
        new TutorialStep { Title = es?"Paso 5: Crear Objeto que Otorgue el Buff":"Step 5: Create Object that Gives Buff", Desc = es?"Para probar, crea un objeto simple en S4S que otorgue tu buff al usarlo. O úsalo en un trait/interaction.":"To test, create a simple object in S4S that gives your buff when used. Or use it in a trait/interaction.", Image="/MVVM/View/logo.png", Label=es?"Objeto":"Object", Color="#F97316" },
        new TutorialStep { Title = es?"Paso 6: Exportar y Probar":"Step 6: Export and Test", Desc = es?"Exporta el mod. En el juego, usa el comando 'buffs.add_buff [BuffName]' para probarlo rápidamente en un Sim activo.":"Export the mod. In-game, use command 'buffs.add_buff [BuffName]' to quickly test it on active Sim.", Image="/MVVM/View/logo.png", Label=es?"¡Listo!":"Done!", Color="#22C55E" }
    };
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            string categoryName = GetCategoryDisplayName(_category, es);
            SubtitleText.Text = es ? $"Aprende a crear: {categoryName}" : $"Learn to create: {categoryName}";

            PrevButton.Content = es ? "← Atrás" : "← Back";
            NextButton.Content = es ? "Siguiente →" : "Next →";
            CloseButton.Content = es ? "Cerrar" : "Close";
            TipsTitle.Text = es ? "💡 Consejos" : "💡 Tips";
        }

        private string GetCategoryDisplayName(string cat, bool es)
        {
            switch (cat)
            {
                case "trait": return es ? "Rasgo" : "Trait";
                case "interaction": return es ? "Interacción" : "Interaction";
                case "career": return es ? "Carrera" : "Career";
                case "object": return es ? "Objeto" : "Object";
                case "clothing": return es ? "Ropa" : "Clothing";
                case "buff": return "Buff";
                default: return cat;
            }
        }

        private void UpdateUI()
        {
            if (_steps == null || _steps.Count == 0) return;

            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            var step = _steps[_currentStep];

            StepIndicator.Text = es
                ? $"Paso {_currentStep + 1} de {_steps.Count}"
                : $"Step {_currentStep + 1} of {_steps.Count}";

            StepTitle.Text = step.Title;
            StepDescription.Text = step.Desc;

            // Tips
            if (!string.IsNullOrEmpty(step.Tips))
            {
                TipsBox.Visibility = Visibility.Visible;
                TipsText.Text = step.Tips;
            }
            else
            {
                TipsBox.Visibility = Visibility.Collapsed;
            }

            // Image
            try
            {
                StepImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(step.Image, UriKind.Relative));
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
            catch
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }

            // Navigation buttons
            PrevButton.Visibility = _currentStep > 0 ? Visibility.Visible : Visibility.Collapsed;

            // Si es el último paso, mostrar "Finalizar" en lugar de "Siguiente"
            if (_currentStep == _steps.Count - 1)
            {
                NextButton.Content = es ? "Finalizar Tutorial" : "Finish Tutorial";
                NextButton.Visibility = Visibility.Visible;
            }
            else
            {
                NextButton.Content = es ? "Siguiente →" : "Next →";
                NextButton.Visibility = Visibility.Visible;
            }

            // Progress dots
            UpdateProgressDots();
        }

        private void UpdateProgressDots()
        {
            ProgressDots.Children.Clear();

            for (int i = 0; i < _steps.Count; i++)
            {
                var dot = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Margin = new Thickness(0, 0, 6, 0),
                    Fill = i == _currentStep
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"))
                };

                if (i == _currentStep)
                {
                    dot.Effect = new DropShadowEffect
                    {
                        Color = (Color)ColorConverter.ConvertFromString("#22C55E"),
                        BlurRadius = 8,
                        ShadowDepth = 0,
                        Opacity = 0.8
                    };
                }

                ProgressDots.Children.Add(dot);
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 0)
            {
                _currentStep--;
                UpdateUI();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep < _steps.Count - 1)
            {
                _currentStep++;

                // Asignar medallas automáticamente según el progreso
                int totalSteps = _steps.Count;
                int bronzeStep = 0; // Primera lección
                int silverStep = totalSteps / 2; // Mitad del tutorial

                if (_currentStep == bronzeStep)
                {
                    ProfileManager.SetTutorialMedal(_tutorialId, MedalType.Bronze);
                    ShowMedalNotification(MedalType.Bronze);
                }
                else if (_currentStep == silverStep)
                {
                    ProfileManager.SetTutorialMedal(_tutorialId, MedalType.Silver);
                    ShowMedalNotification(MedalType.Silver);
                }

                UpdateUI();
            }
            else
            {
                // Prevenir spam - verificar si ya está mostrando medalla
                if (_isShowingMedal) return;

                // Tutorial completado - otorgar medalla de oro
                ProfileManager.SetTutorialMedal(_tutorialId, MedalType.Gold);
                ShowMedalNotification(MedalType.Gold);

                // Cerrar la ventana después de que se cierre el popup de medalla
                // El popup ya maneja el cierre automático después de 3 segundos
            }
        }

        private void ShowMedalNotification(MedalType medal)
        {
            // Prevenir spam de medallas
            if (_isShowingMedal) return;
            _isShowingMedal = true;

            // Deshabilitar todos los botones
            NextButton.IsEnabled = false;
            PrevButton.IsEnabled = false;
            CloseButton.IsEnabled = false;

            // Si es medalla de oro, cerrar inmediatamente la ventana
            if (medal == MedalType.Gold)
            {
                // Mostrar popup de medalla
                var medalPopup = new MedalPopupView(medal);
                medalPopup.Show();

                // Cerrar INMEDIATAMENTE esta ventana sin esperar
                this.Close();
            }
            else
            {
                // Para bronce y plata, esperar a que se cierre el popup
                var medalPopup = new MedalPopupView(medal);
                medalPopup.Closed += (s, e) =>
                {
                    // Re-habilitar botones cuando se cierre el popup
                    NextButton.IsEnabled = true;
                    PrevButton.IsEnabled = true;
                    CloseButton.IsEnabled = true;
                    _isShowingMedal = false;
                };
                medalPopup.Show();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}