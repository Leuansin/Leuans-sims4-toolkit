import os
import hashlib
import threading
import json
from datetime import datetime
import customtkinter as ctk
from tkinter import filedialog, messagebox

class Sims4HasherApp(ctk.CTk):
    def __init__(self):
        super().__init__()

        self.title("Sims 4 Integrity Auditor + JSON Export")
        self.geometry("750(x)550")
        ctk.set_appearance_mode("dark")
        ctk.set_default_color_theme("blue")

        self.default_path = r"C:\Program Files (x86)\Steam\steamapps\common\The Sims 4"
        self.target_folders = ["__Installer", "Data", "Delta", "Game", "Support"]
        
        # Diccionario maestro para el JSON
        self.master_data = {}

        # --- UI LAYOUT ---
        self.grid_columnconfigure(0, weight=1)
        self.grid_rowconfigure(3, weight=1)

        self.label = ctk.CTkLabel(self, text="TS4 Hash & JSON Generator", font=("Segoe UI", 24, "bold"))
        self.label.grid(row=0, column=0, padx=20, pady=20)

        self.path_frame = ctk.CTkFrame(self)
        self.path_frame.grid(row=1, column=0, padx=20, pady=10, sticky="ew")
        self.path_frame.grid_columnconfigure(0, weight=1)

        self.path_entry = ctk.CTkEntry(self.path_frame, placeholder_text="Ruta del juego...")
        self.path_entry.insert(0, self.default_path)
        self.path_entry.grid(row=0, column=0, padx=10, pady=10, sticky="ew")

        self.browse_btn = ctk.CTkButton(self.path_frame, text="Buscar", width=100, command=self.browse_path)
        self.browse_btn.grid(row=0, column=1, padx=10, pady=10)

        self.start_btn = ctk.CTkButton(self, text="GENERAR REPORTES Y JSON", height=45, fg_color="#2ecc71", hover_color="#27ae60", font=("Segoe UI", 14, "bold"), command=self.start_process)
        self.start_btn.grid(row=2, column=0, padx=20, pady=10, sticky="ew")

        self.log_box = ctk.CTkTextbox(self, font=("Consolas", 12))
        self.log_box.grid(row=3, column=0, padx=20, pady=20, sticky="nsew")

    def log(self, text):
        self.log_box.insert("end", f"{text}\n")
        self.log_box.see("end")

    def browse_path(self):
        dir_selected = filedialog.askdirectory()
        if dir_selected:
            self.path_entry.delete(0, "end")
            self.path_entry.insert(0, dir_selected)

    def is_target_folder(self, folder_name):
        prefixes = ("EP", "GP", "SP", "FP")
        return folder_name in self.target_folders or folder_name.startswith(prefixes)

    def calculate_sha256(self, file_path):
        sha256_hash = hashlib.sha256()
        try:
            with open(file_path, "rb") as f:
                for byte_block in iter(lambda: f.read(8192), b""): # Bloque de 8KB para velocidad
                    sha256_hash.update(byte_block)
            return sha256_hash.hexdigest()
        except Exception:
            return None

    def run_hashing(self):
        base_dir = self.path_entry.get()
        if not os.path.exists(base_dir):
            messagebox.showerror("Error", "Ruta no válida")
            self.start_btn.configure(state="normal")
            return

        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        output_folder = f"Sims4_Integrity_{timestamp}"
        os.makedirs(output_folder, exist_ok=True)
        
        self.master_data = {
            "info": {
                "game": "The Sims 4",
                "date": timestamp,
                "base_path": base_dir
            },
            "files": {}
        }

        try:
            top_folders = [d for d in os.listdir(base_dir) if os.path.isdir(os.path.join(base_dir, d))]
        except Exception as e:
            self.log(f"Error: {e}")
            return

        for folder in top_folders:
            if self.is_target_folder(folder):
                full_folder_path = os.path.join(base_dir, folder)
                txt_filename = os.path.join(output_folder, f"Hashes_{folder}.txt")
                
                self.log(f"[>] Escaneando: {folder}...")
                
                with open(txt_filename, "w", encoding="utf-8") as out_file:
                    out_file.write(f"TS4 FOLDER: {folder}\n\n")

                    for root, _, files in os.walk(full_folder_path):
                        for file in files:
                            file_path = os.path.join(root, file)
                            # Calculamos ruta relativa para el JSON (ej: Game/Bin/TS4.exe)
                            rel_path = os.path.relpath(file_path, base_dir)
                            
                            file_hash = self.calculate_sha256(file_path)
                            
                            if file_hash:
                                # Guardar en JSON
                                self.master_data["files"][rel_path] = file_hash
                                # Guardar en TXT
                                out_file.write(f"{rel_path} | {file_hash}\n")

                self.log(f"[OK] {folder} completado.")

        # Guardar el JSON Maestro
        json_path = os.path.join(output_folder, "master_hashes.json")
        with open(json_path, "w", encoding="utf-8") as jf:
            json.dump(self.master_data, jf, indent=4)

        self.log(f"\n[EXITO] JSON generado: {json_path}")
        self.start_btn.configure(state="normal")
        messagebox.showinfo("Proceso Finalizado", "Se han generado los archivos .txt y el master_hashes.json")

    def start_process(self):
        self.start_btn.configure(state="disabled")
        self.log_box.delete("1.0", "end")
        threading.Thread(target=self.run_hashing, daemon=True).start()

if __name__ == "__main__":
    app = Sims4HasherApp()
    app.mainloop()
