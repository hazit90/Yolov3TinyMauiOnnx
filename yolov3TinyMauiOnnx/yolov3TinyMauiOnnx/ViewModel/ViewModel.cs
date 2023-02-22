using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

using Image = SixLabors.ImageSharp.Image;


namespace yolov3TinyMauiOnnx.ViewModel;

public class ViewModel:  INotifyPropertyChanged
{
    #region PropertyChangedSetup
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    #endregion

    private Model.YoloOnnxModel yolov3Tiny;
    private Model.DebugManagement debugMan;

    #region PrivateBindings
    private string b_ImageHeading;
    private int b_Counter = 0;
    private ImageSource b_ViewFinderSource = null;
    private string b_DebugString;
    private static int counter = 0;
    #endregion

    #region BindedProperties
    public string B_ImageHeading
    {
        get
        {
            return b_ImageHeading;
        }
        set
        {
            b_ImageHeading = value;
            OnPropertyChanged();
        }
    }              
    public ICommand B_ButtonCommand { get; private set; }
    public ICommand B_ConsoleTest { get; private set; }
    public ImageSource B_ViewFinderSource
    {
        get
        {
            return b_ViewFinderSource;
        }
        set
        {
            b_ViewFinderSource = value;
            OnPropertyChanged();
        }
    }
    public string B_DebugString
    {
        get
        {
            return b_DebugString;
        }
        set
        {
            b_DebugString = value;
            OnPropertyChanged();
        }
    }
    public int B_Counter
    {
        get
        {
            return counter;
        }
        set
        {
            counter = value;
            OnPropertyChanged();
        }
    }
    #endregion

    public ViewModel()
    {
        B_ImageHeading = "YoloV3-Tiny";
        B_ButtonCommand = new Command(() => GrabImageClicked());
        B_ConsoleTest = new Command(() => ConsoleTest());

        debugMan = new Model.DebugManagement();
        debugMan.PropertyChanged += B_DebugMan_PropertyChanged;

        string modelPath = "tiny-yolov3-11.onnx";
        yolov3Tiny = new Model.YoloOnnxModel(debugMan, modelPath);
    }

    private void ConsoleTest()
    {
        B_Counter++;
    }

    private void B_DebugMan_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(debugMan.B_DebugString))
        {
            this.B_DebugString = debugMan.B_DebugString;
        }
    }

    private async void GrabImageClicked()
    {
        var imageBytes = await TakePhoto();           

        //SetToViewFinder(new MemoryStream(imageBytes));

        Image<Rgb24> imageBoxesDrawn = yolov3Tiny.RunInference(imageBytes);
        SetToViewFinder(imageBoxesDrawn);
    }

    private void SetToViewFinder(Image<Rgb24> data)
    {
        // memory stream
        var memoryStream = new MemoryStream();

        var encoder = new JpegEncoder()
        {
            Quality = 30 //Use variable to set between 5-30 based on your requirements
        };

        //This saves to the memoryStream with encoder
        data.Save(memoryStream, encoder);
        memoryStream.Position = 0;


        B_ViewFinderSource = ImageSource.FromStream(() => memoryStream);

    }

    private async Task<byte[]> TakePhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                using var sourceStream = await photo.OpenReadAsync();
                debugMan.SetDebugMessage("photo taken");

                return ReadFully(sourceStream);
            }
        }
        return null;
    }

    private async Task<Stream> TakePhotoGetStream()
    {

        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                var sourceStream = await photo.OpenReadAsync();
                return sourceStream;
            }
        }
        return null;
    }

    private static byte[] ReadFully(Stream input)
    {
        byte[] buffer = new byte[16 * 1024];
        using (MemoryStream ms = new MemoryStream())
        {
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
    }

    private void SetToViewFinder(byte[] data)
    {
        var stream = new MemoryStream(data);            
        
        B_ViewFinderSource = ImageSource.FromStream(() => stream);
    }

    private void SetToViewFinder(Stream stream)
    {         
        B_ViewFinderSource = ImageSource.FromStream(() => stream);
    }

    public static MemoryStream CloneMemoryStream(MemoryStream input)
    {
        MemoryStream retVal = new MemoryStream();

        input.CopyTo(retVal);
        return retVal;
    }

}

