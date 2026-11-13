using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeonAudioPlayer;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _positionTimer;
    private bool _isSeeking;
    private bool _updatingTimeline;
    private bool _isPaused;
    private bool _hasSource;
    private bool _hasDuration;

    public MainWindow()
    {
        InitializeComponent();
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _positionTimer.Tick += (_, _) => UpdateTimeline();
        AudioEngine.Volume = VolumeSlider.Value / 100d;
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select an audio signal",
            Filter = "Audio files|*.mp3;*.wav;*.wma;*.m4a|MP3 audio|*.mp3|Wave audio|*.wav|Windows Media Audio|*.wma|MPEG-4 audio|*.m4a|All files|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != true) return;

        AudioEngine.Stop();
        AudioEngine.Source = new Uri(dialog.FileName);
        TrackTitleText.Text = Path.GetFileNameWithoutExtension(dialog.FileName).ToUpperInvariant();
        TrackTitleText.ToolTip = dialog.FileName;
        StatusText.Text = "LOADING";
        PauseResumeText.Text = "PAUSE";
        _hasSource = true;
        _hasDuration = false;
        _isPaused = false;
        SeekSlider.IsEnabled = false;
        SeekSlider.Value = 0;
        CurrentTimeText.Text = "00:00";
        DurationText.Text = "00:00";
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSource()) return;
        AudioEngine.Play();
        _isPaused = false;
        PauseResumeText.Text = "PAUSE";
        StatusText.Text = "PLAYING";
        _positionTimer.Start();
    }

    private void PauseResume_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSource()) return;
        if (_isPaused)
        {
            AudioEngine.Play();
            StatusText.Text = "PLAYING";
            PauseResumeText.Text = "PAUSE";
            _positionTimer.Start();
        }
        else
        {
            AudioEngine.Pause();
            StatusText.Text = "PAUSED";
            PauseResumeText.Text = "RESUME";
            _positionTimer.Stop();
        }
        _isPaused = !_isPaused;
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSource()) return;
        AudioEngine.Stop();
        _positionTimer.Stop();
        _isPaused = false;
        PauseResumeText.Text = "PAUSE";
        StatusText.Text = "READY";
        SetTimelinePosition(TimeSpan.Zero);
    }

    private void AudioEngine_MediaOpened(object sender, RoutedEventArgs e)
    {
        _hasDuration = AudioEngine.NaturalDuration.HasTimeSpan;
        SeekSlider.IsEnabled = PlayerLogic.CanSeek(_hasSource, _hasDuration);
        if (_hasDuration)
        {
            SeekSlider.Maximum = AudioEngine.NaturalDuration.TimeSpan.TotalSeconds;
            DurationText.Text = PlayerLogic.FormatTime(AudioEngine.NaturalDuration.TimeSpan);
        }
        StatusText.Text = "READY";
    }

    private void AudioEngine_MediaEnded(object sender, RoutedEventArgs e)
    {
        AudioEngine.Stop();
        _positionTimer.Stop();
        _isPaused = false;
        PauseResumeText.Text = "PAUSE";
        StatusText.Text = "READY";
        SetTimelinePosition(TimeSpan.Zero);
    }

    private void AudioEngine_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        _positionTimer.Stop();
        StatusText.Text = "ERROR";
        SeekSlider.IsEnabled = false;
        MessageBox.Show(this, e.ErrorException?.Message ?? "The audio signal could not be decoded.", "Neon Console", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void SeekSlider_DragStarted(object sender, MouseButtonEventArgs e) => _isSeeking = true;

    private void SeekSlider_DragCompleted(object sender, MouseButtonEventArgs e)
    {
        if (PlayerLogic.CanSeek(_hasSource, _hasDuration))
            AudioEngine.Position = TimeSpan.FromSeconds(SeekSlider.Value);
        _isSeeking = false;
    }

    private void SeekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (CurrentTimeText is not null)
            CurrentTimeText.Text = PlayerLogic.FormatTime(TimeSpan.FromSeconds(e.NewValue));
        if (!_updatingTimeline && !_isSeeking && AudioEngine is not null && PlayerLogic.CanSeek(_hasSource, _hasDuration))
            AudioEngine.Position = TimeSpan.FromSeconds(e.NewValue);
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (AudioEngine is not null) AudioEngine.Volume = e.NewValue / 100d;
        if (VolumeText is not null) VolumeText.Text = $"{e.NewValue:0}%";
    }

    private void SkinButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string tag }) return;
        var parts = tag.Split('|');
        if (parts.Length != 5) return;
        var keys = new[] { "NeonPrimary", "NeonSecondary", "NeonAccent", "NeonDim" };
        for (var i = 0; i < keys.Length; i++)
            Application.Current.Resources[keys[i]] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(parts[i + 1]));
        var glowColor = (Color)ColorConverter.ConvertFromString(parts[1]);
        Application.Current.Resources["SoftGlow"] = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = glowColor, BlurRadius = 18, ShadowDepth = 0, Opacity = 0.72
        };
        Application.Current.Resources["StrongGlow"] = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = glowColor, BlurRadius = 28, ShadowDepth = 0, Opacity = 0.95
        };
        ActiveSkinText.Text = parts[0].ToUpperInvariant();
    }

    private void WindowHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space) PauseResume_Click(sender, e);
        else if (e.Key == Key.Escape) Stop_Click(sender, e);
        else if (e.Key == Key.O && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) OpenFile_Click(sender, e);
    }

    private bool EnsureSource()
    {
        if (_hasSource) return true;
        StatusText.Text = "NO SOURCE";
        return false;
    }

    private void UpdateTimeline()
    {
        if (!_hasSource || !_hasDuration || _isSeeking) return;
        SetTimelinePosition(AudioEngine.Position);
    }

    private void SetTimelinePosition(TimeSpan position)
    {
        _updatingTimeline = true;
        SeekSlider.Value = position.TotalSeconds;
        CurrentTimeText.Text = PlayerLogic.FormatTime(position);
        _updatingTimeline = false;
    }
}
