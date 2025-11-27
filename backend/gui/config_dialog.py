"""
GUI de configuração inicial para Etnopapers standalone

Solicita MONGO_URI na primeira execução e salva em arquivo .env

Usa Tkinter (incluído no Python) para máxima compatibilidade cross-platform.
"""

import tkinter as tk
from tkinter import messagebox, font
import re
from pathlib import Path


def validate_mongo_uri(uri: str) -> bool:
    """
    Valida formato básico de MongoDB URI

    Args:
        uri: String de conexão MongoDB

    Returns:
        True se formato válido, False caso contrário
    """
    if not uri or len(uri.strip()) == 0:
        return False

    # Padrões válidos:
    # mongodb://...
    # mongodb+srv://...
    patterns = [
        r'^mongodb://[\w\-\.]+.*',
        r'^mongodb\+srv://[\w\-\.]+.*',
    ]

    return any(re.match(pattern, uri.strip()) for pattern in patterns)


def show_config_dialog():
    """
    Mostra dialog de configuração inicial

    Solicita MONGO_URI e salva em .env
    """
    root = tk.Tk()
    root.title("Etnopapers - Configuração Inicial")
    root.geometry("700x500")
    root.resizable(False, False)

    # Configurar estilo
    default_font = font.nametofont("TkDefaultFont")
    default_font.configure(size=10)

    # Frame principal com padding
    main_frame = tk.Frame(root, padx=30, pady=30)
    main_frame.pack(fill=tk.BOTH, expand=True)

    # Título
    title_label = tk.Label(
        main_frame,
        text="Etnopapers - Configuração Inicial",
        font=("Arial", 16, "bold")
    )
    title_label.pack(pady=(0, 10))

    # Subtítulo
    subtitle_label = tk.Label(
        main_frame,
        text="Configure a conexão com o banco de dados MongoDB",
        font=("Arial", 10)
    )
    subtitle_label.pack(pady=(0, 20))

    # Separator
    separator = tk.Frame(main_frame, height=2, bd=1, relief=tk.SUNKEN)
    separator.pack(fill=tk.X, padx=5, pady=10)

    # Label MongoDB URI
    mongo_label = tk.Label(
        main_frame,
        text="MongoDB URI:",
        font=("Arial", 11, "bold"),
        anchor="w"
    )
    mongo_label.pack(fill=tk.X, pady=(10, 5))

    # Entry MongoDB URI
    mongo_entry = tk.Entry(main_frame, font=("Arial", 10), width=60)
    mongo_entry.pack(fill=tk.X, pady=(0, 10))

    # Placeholder text
    mongo_entry.insert(0, "mongodb://localhost:27017/etnopapers")
    mongo_entry.config(fg='gray')

    def on_entry_click(event):
        """Remove placeholder text on click"""
        if mongo_entry.get() == "mongodb://localhost:27017/etnopapers":
            mongo_entry.delete(0, tk.END)
            mongo_entry.config(fg='black')

    def on_focusout(event):
        """Restore placeholder if empty"""
        if mongo_entry.get() == '':
            mongo_entry.insert(0, "mongodb://localhost:27017/etnopapers")
            mongo_entry.config(fg='gray')

    mongo_entry.bind('<FocusIn>', on_entry_click)
    mongo_entry.bind('<FocusOut>', on_focusout)

    # Help text
    help_frame = tk.Frame(main_frame, bg='#f0f0f0', padx=15, pady=15)
    help_frame.pack(fill=tk.X, pady=(5, 15))

    help_title = tk.Label(
        help_frame,
        text="Exemplos de URI:",
        font=("Arial", 9, "bold"),
        bg='#f0f0f0',
        anchor="w"
    )
    help_title.pack(fill=tk.X)

    help_examples = tk.Label(
        help_frame,
        text=(
            "• MongoDB local:\n"
            "  mongodb://localhost:27017/etnopapers\n\n"
            "• MongoDB Atlas (cloud):\n"
            "  mongodb+srv://user:password@cluster.mongodb.net/etnopapers\n\n"
            "• MongoDB com autenticação:\n"
            "  mongodb://username:password@localhost:27017/etnopapers"
        ),
        font=("Courier", 9),
        bg='#f0f0f0',
        justify=tk.LEFT,
        anchor="w"
    )
    help_examples.pack(fill=tk.X, pady=(5, 0))

    # Status label
    status_label = tk.Label(
        main_frame,
        text="",
        fg="red",
        font=("Arial", 9)
    )
    status_label.pack(pady=(5, 10))

    # Botões
    button_frame = tk.Frame(main_frame)
    button_frame.pack(pady=(10, 0))

    def save_config():
        """Valida e salva configuração"""
        mongo_uri = mongo_entry.get()

        # Remover placeholder text
        if mongo_uri == "mongodb://localhost:27017/etnopapers" and mongo_entry.cget('fg') == 'gray':
            mongo_uri = ""

        # Validar
        if not validate_mongo_uri(mongo_uri):
            status_label.config(
                text="⚠ URI inválido. Use formato: mongodb://... ou mongodb+srv://...",
                fg="red"
            )
            mongo_entry.config(bg='#ffcccc')
            return

        # Salvar em .env
        try:
            env_file = Path('.env')
            with open(env_file, 'w', encoding='utf-8') as f:
                f.write(f"# Etnopapers - Configuração\n")
                f.write(f"# Gerado automaticamente em {Path.cwd()}\n\n")
                f.write(f"MONGO_URI={mongo_uri}\n")
                f.write(f"OLLAMA_URL=http://localhost:11434\n")
                f.write(f"OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M\n")

            messagebox.showinfo(
                "Sucesso",
                f"Configuração salva com sucesso!\n\nArquivo: {env_file.absolute()}"
            )
            root.destroy()

        except Exception as e:
            messagebox.showerror(
                "Erro",
                f"Erro ao salvar configuração:\n{str(e)}"
            )

    def cancel():
        """Cancela e fecha aplicação"""
        if messagebox.askyesno("Cancelar", "Deseja sair sem configurar?"):
            root.destroy()
            import sys
            sys.exit(0)

    # Botão Salvar
    save_button = tk.Button(
        button_frame,
        text="Salvar e Iniciar",
        command=save_config,
        font=("Arial", 10, "bold"),
        bg="#4CAF50",
        fg="white",
        padx=20,
        pady=8,
        cursor="hand2"
    )
    save_button.pack(side=tk.LEFT, padx=5)

    # Botão Cancelar
    cancel_button = tk.Button(
        button_frame,
        text="Cancelar",
        command=cancel,
        font=("Arial", 10),
        bg="#f44336",
        fg="white",
        padx=20,
        pady=8,
        cursor="hand2"
    )
    cancel_button.pack(side=tk.LEFT, padx=5)

    # Centralizar janela na tela
    root.update_idletasks()
    width = root.winfo_width()
    height = root.winfo_height()
    x = (root.winfo_screenwidth() // 2) - (width // 2)
    y = (root.winfo_screenheight() // 2) - (height // 2)
    root.geometry(f'{width}x{height}+{x}+{y}')

    # Focus no entry
    mongo_entry.focus_set()

    # Bind Enter key para salvar
    root.bind('<Return>', lambda e: save_config())

    # Run GUI
    root.mainloop()


if __name__ == '__main__':
    # Teste standalone
    show_config_dialog()
