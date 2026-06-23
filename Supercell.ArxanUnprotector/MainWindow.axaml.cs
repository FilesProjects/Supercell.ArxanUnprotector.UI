using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supercell.ArxanUnprotector.Actions;

namespace Supercell.ArxanUnprotector;

public partial class MainWindow : Window
{
    //конструктор
    public MainWindow()
    {
        InitializeComponent();
        
        //события
        ActionSelector.SelectionChanged += ActionChanged;
        OriginalBrowseBtn.Click += OriginalBrowse;
        ModifiedBrowseBtn.Click += ModifiedBrowse;
        OutputBrowseBtn.Click += OutputBrowse;
        ExecuteBtn.Click += RunAction;
        
        //перетаскивание файлов
        OriginalPathText.AddHandler(DragDrop.DragOverEvent, DragOver);
        OriginalPathText.AddHandler(DragDrop.DropEvent, OriginalDrop);
        ModifiedPathText.AddHandler(DragDrop.DragOverEvent, DragOver);
        ModifiedPathText.AddHandler(DragDrop.DropEvent, ModifiedDrop);
        
        //перенаправление консоли
        Console.SetOut(new LogWriter(Log));
        
        //обновление вида
        UpdateUi();
    }
    
    //выбор действия
    private void ActionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateUi();
    }
    
    //обновление интерфейса
    private void UpdateUi()
    {
        if (ActionSelector == null) return;
        
        var i = ActionSelector.SelectedIndex;
        
        // decrypt
        if (i == 0)
        {
            ModifiedPanel.IsVisible = false;
            OutputPanel.IsVisible = true;
        }
        // encrypt
        else if (i == 1)
        {
            ModifiedPanel.IsVisible = true;
            ModifiedLabel.Text = "Decrypted Library File (-m)";
            OutputPanel.IsVisible = true;
        }
        // update-crc
        else if (i == 2)
        {
            ModifiedPanel.IsVisible = true;
            ModifiedLabel.Text = "Modified Library File (-m)";
            OutputPanel.IsVisible = true;
        }
        // verify-crc
        else if (i == 3)
        {
            ModifiedPanel.IsVisible = true;
            ModifiedLabel.Text = "Modified Library File (-m)";
            OutputPanel.IsVisible = false;
        }
    }
    
    //выбор оригинального файла
    private async void OriginalBrowse(object sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Original Library File",
            AllowMultiple = false
        });
        
        if (files.Any())
        {
            OriginalPathText.Text = files[0].Path.LocalPath;
        }
    }
    
    //выбор измененного файла
    private async void ModifiedBrowse(object sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Modified/Decrypted Library File",
            AllowMultiple = false
        });
        
        if (files.Any())
        {
            ModifiedPathText.Text = files[0].Path.LocalPath;
        }
    }
    
    //выбор папки сохранения
    private async void OutputBrowse(object sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Save Folder",
            AllowMultiple = false
        });
        
        if (folders.Any())
        {
            OutputPathText.Text = folders[0].Path.LocalPath;
        }
    }
    
    //перетаскивание файла
    private void DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
            e.DragEffects = DragDropEffects.Copy;
        else
            e.DragEffects = DragDropEffects.None;
    }
    
    //сброс оригинального файла
    private void OriginalDrop(object sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();
        if (files != null && files.Any())
        {
            OriginalPathText.Text = files.First().Path.LocalPath;
        }
    }
    
    //сброс измененного файла
    private void ModifiedDrop(object sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();
        if (files != null && files.Any())
        {
            ModifiedPathText.Text = files.First().Path.LocalPath;
        }
    }
    
    //вывод лога
    private void Log(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (LogText != null)
            {
                LogText.Text += text;
                LogText.CaretIndex = LogText.Text.Length;
            }
        });
    }
    
    //очистка лога
    private void ClearLog()
    {
        if (LogText != null) LogText.Text = string.Empty;
    }
    
    //запуск действия
    private async void RunAction(object sender, RoutedEventArgs e)
    {
        ClearLog();
        
        var actIndex = ActionSelector.SelectedIndex;
        var original = OriginalPathText.Text;
        var modified = ModifiedPathText.Text;
        var outputFolder = OutputPathText.Text;
        
        //проверка входных данных
        if (string.IsNullOrWhiteSpace(original) || !File.Exists(original))
        {
            Log("Error: Original file does not exist.\r\n");
            return;
        }
        
        if (actIndex > 0 && (string.IsNullOrWhiteSpace(modified) || !File.Exists(modified)))
        {
            Log("Error: Modified/Decrypted file does not exist.\r\n");
            return;
        }
        
        //блокировка кнопки на время работы
        ExecuteBtn.IsEnabled = false;
        Log($"[{DateTime.Now:HH:mm:ss}] Action started...\r\n");
        
        try
        {
            await Task.Run(() =>
            {
                //определяем пути
                string outPath = null;
                IAction action = null;
                
                if (actIndex == 0) // decrypt
                {
                    action = new DecryptStringsAction();
                    if (string.IsNullOrWhiteSpace(outputFolder))
                        outPath = original + ".decrypted";
                    else
                        outPath = Path.Combine(outputFolder, Path.GetFileName(original) + ".decrypted");
                }
                else if (actIndex == 1) // encrypt
                {
                    action = new EncryptStringsAction();
                    if (string.IsNullOrWhiteSpace(outputFolder))
                        outPath = modified + ".encrypted";
                    else
                        outPath = Path.Combine(outputFolder, Path.GetFileName(modified) + ".encrypted");
                }
                else if (actIndex == 2) // update-crc
                {
                    action = new UpdateChecksumsAction();
                    if (string.IsNullOrWhiteSpace(outputFolder))
                        outPath = modified;
                    else
                        outPath = Path.Combine(outputFolder, Path.GetFileName(modified));
                }
                else if (actIndex == 3) // verify-crc
                {
                    action = new VerifyChecksumsAction();
                    outPath = null;
                }
                
                if (action == null)
                {
                    Log("Error: Invalid action selected.\r\n");
                    return;
                }
                
                //загрузка библиотек
                Log("Loading libraries...\r\n");
                Library origLib = File.Exists(original) ? LibraryLoader.Load(original) : null;
                Library modLib = File.Exists(modified) ? LibraryLoader.Load(modified) : null;
                
                //создание выходной директории
                if (outPath != null)
                {
                    string dir = Path.GetDirectoryName(outPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                
                //выполнение действия
                Log("Running logic...\r\n");
                string result = action.Execute(origLib, modLib, outPath);
                
                if (result == null)
                {
                    Log("Success!\r\n");
                    if (outPath != null)
                    {
                        Log($"Saved file to: {outPath}\r\n");
                    }
                }
                else
                {
                    Log($"Result: {result}\r\n");
                }
            });
        }
        catch (Exception ex)
        {
            Log($"Exception: {ex.Message}\r\n{ex.StackTrace}\r\n");
        }
        finally
        {
            Dispatcher.UIThread.Post(() =>
            {
                ExecuteBtn.IsEnabled = true;
            });
            Log($"[{DateTime.Now:HH:mm:ss}] Action finished.\r\n");
        }
    }
}

//класс перенаправления консоли
internal class LogWriter : TextWriter
{
    private readonly Action<string> _log;
    
    public LogWriter(Action<string> log)
    {
        _log = log;
    }
    
    public override Encoding Encoding => Encoding.UTF8;
    
    public override void Write(char v)
    {
        _log(v.ToString());
    }
    
    public override void Write(string v)
    {
        if (v != null) _log(v);
    }
    
    public override void WriteLine(string v)
    {
        if (v != null) _log(v + Environment.NewLine);
    }
}
