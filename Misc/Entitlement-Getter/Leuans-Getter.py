import requests
import re
import os

# ==========================================
# ⚙️  CONFIGURACIÓN
# ==========================================
nombre_dlc = "royalty and legacy grand bundle"
numero_ini = "142"                   
codigo_sp  = "EP21"                  
# ==========================================

def limpiar_nombre_a_mano(nombre):
    # Método de respaldo: Limpieza manual estilo Anadius
    nombre_limpio = nombre.replace("The Sims™ 4", "").replace("The Sims 4", "").replace("™", "")
    nombre_limpio = nombre_limpio.replace("&", "And")
    
    # Borrar sufijos
    sufijos = [" Kit", " Stuff Pack", " Stuff", " Game Pack", " Expansion Pack"]
    for s in sufijos:
        nombre_limpio = re.sub(re.escape(s) + "$", "", nombre_limpio, flags=re.IGNORECASE)
        
    return nombre_limpio.replace(" ", "").strip()

def extraer_nombre_interno_de_url(url):
    # Intenta sacar el nombre interno real desde la URL de EA
    # Ejemplo URL: .../addons/the-sims-4-garden-to-table-kit
    try:
        # Cogemos la última parte de la URL
        slug = url.split("/")[-1]
        
        # Quitamos "the-sims-4-" del principio
        slug = slug.replace("the-sims-4-", "")
        
        # Quitamos "-kit", "-stuff-pack", etc del final
        slug = re.sub(r'-(kit|stuff-pack|game-pack|expansion-pack)$', '', slug)
        
        # Convertimos "garden-to-table" en "GardentoTable" (CamelCase falso)
        partes = slug.split("-")
        nombre_interno = "".join([p.capitalize() for p in partes])
        
        # Corrección: A veces EA pone todo en minuscula en la URL, pero el ETG usa CamelCase.
        # El método de "limpiar_nombre_a_mano" suele respetar mejor las mayúsculas originales.
        # Así que usaremos la URL solo para confirmar la estructura.
        return nombre_interno
    except:
        return None

def buscar_y_generar():
    print(f"\n☠️  INICIANDO OPERACIÓN PARA: '{nombre_dlc}'")
    
    # 1. Preparar URL de búsqueda
    slug_busqueda = nombre_dlc.lower().replace(" ", "-")
    urls_posibles = [
        f"https://www.ea.com/games/the-sims/the-sims-4/store/addons/the-sims-4-{slug_busqueda}",
        f"https://www.ea.com/games/the-sims/the-sims-4/store/addons/the-sims-4-{slug_busqueda}-kit",
        f"https://www.ea.com/en-us/games/the-sims/the-sims-4/store/addons/{slug_busqueda}"
    ]

    headers = {"User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"}
    
    html_encontrado = ""
    url_final = ""

    # 2. Encontrar la web
    for url in urls_posibles:
        try:
            print(f"Testing URL: {url} ...", end=" ")
            r = requests.get(url, headers=headers, timeout=10)
            if r.status_code == 200:
                print("✅ ¡ENCONTRADA!")
                html_encontrado = r.text
                url_final = r.url # La URL real final (después de redirecciones)
                break
            else:
                print(f"❌ ({r.status_code})")
        except Exception as e:
            print(f"Error: {e}")

    if not html_encontrado:
        print("\n❌ No se encontró la página. Revisa el nombre.")
        return

    # 3. Extraer IID
    print("\n🧠 Extrayendo IID...")
    patron_iid = r'"offerId":"(SIMS4\.OFF\.SOLP\.0x[A-Fa-f0-9]+)"'
    match_iid = re.search(patron_iid, html_encontrado)
    
    if not match_iid:
        # Intento secundario
        patron_emergencia = r'SIMS4\.OFF\.SOLP\.0x[A-Fa-f0-9]+'
        match_iid = re.search(patron_emergencia, html_encontrado)
        if match_iid:
            iid_completo = match_iid.group(0)
        else:
            print("❌ No se encontró el IID.")
            return
    else:
        iid_completo = match_iid.group(1)

    # 4. Calcular Decimal
    hex_part = iid_completo.split('.')[-1].replace("0x", "")
    decimal_calc = int(hex_part, 16)
    print(f"✅ IID Hex: {iid_completo}")
    print(f"✅ Decimal: {decimal_calc}")

    # 5. Generar Nombre Interno (Lógica Híbrida)
    # Preferimos el método de limpieza manual porque respeta las Mayúsculas del título original
    # (La URL suele estar todo en minúsculas, y el ETG queda más bonito con mayúsculas: GardentoTable vs gardentotable)
    nombre_interno = limpiar_nombre_a_mano(nombre_dlc)
    
    print(f"✅ Nombre Interno generado: {nombre_interno}")

    # 6. Crear Archivo
    bloque_config = (
        f"NAM{numero_ini}=The Sims™ 4 {nombre_dlc}\n"
        f"IID{numero_ini}={iid_completo}\n"
        f"ETG{numero_ini}={codigo_sp}_{nombre_interno}_0x{hex_part}:{decimal_calc}\n"
        f"GRP{numero_ini}=THESIMS4PC\n"
        f"TYP{numero_ini}=DEFAULT"
    )

    nombre_archivo = f"{nombre_dlc}.txt"
    with open(nombre_archivo, "w", encoding="utf-8") as f:
        f.write(bloque_config)
    
    print("\n" + "█" * 50)
    print(f"   ¡ARCHIVO GENERADO: {nombre_archivo}!")
    print("█" * 50)
    print(bloque_config)

if __name__ == "__main__":
    buscar_y_generar()
