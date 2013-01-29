module MainApp

open System
open System.Windows
open System.Windows.Controls
open FSharpx
open System.Net
open System.Text
open FsJson
open FsJsonDescription

type MainWindow = XAML<"MainWindow.xaml">

let api = "https://api.weibo.com/2/"
let token = "access_token=2.00kQ8bDC7MZBJD6cb03781ccJH3hzD"

type WeiboMessage (status : Json) =
    let visible = function
                  | JsonNull -> Visibility.Collapsed
                  | _ -> Visibility.Visible
    member x.Text = status ? text.Val
    member x.User = "@" + status ? user ? screen_name.Val
    member x.UserPic = status ? user ? profile_image_url.Val

    member x.SmallPic = status ? thumbnail_pic.Val
    member x.NormalPic = status ? bmiddle_pic.Val
    member x.OriginalPic = status ? original_pic.Val

    member x.Retweet = new WeiboMessage(status ? retweeted_status)

    member x.MediaVis = status ? thumbnail_pic |> visible
    member x.RetweetVis = status ? retweeted_status |> visible

//let rec TimeLine req count = seq {
//    let uri = new Uri(sprintf "%s%s?%s&count=%d" api req token count)
//    let wc = new WebClient()
//    let resp = wc.DownloadData uri |> Encoding.UTF8.GetString
//    let tl = parse resp
//    yield! tl ? statuses.Array |> Seq.map(fun x -> new WeiboMessage(x))
//    yield! TimeLine req count
//}

let updateTimeLine (w:MainWindow) =
    async {
        let uri = new Uri(api + "statuses/home_timeline.json?" + token + "&count=50")
        let wc = new WebClient()
        wc.DownloadDataAsync uri
        let! res = Async.AwaitEvent wc.DownloadDataCompleted
        let resp = match res.Error with
                   | null -> res.Result |> Encoding.UTF8.GetString
                   | ex -> raise ex
        let tl = parse resp
        
        Application.Current.Dispatcher.InvokeAsync (fun () ->
            let data = tl?statuses.Array |> Seq.map (fun x -> new WeiboMessage(x))
            w.Lv.DataContext <- data :> obj
        ) |> ignore
    } |> Async.Start

let loadWindow() =
   let window = MainWindow()
   updateTimeLine window
   window.Root

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore
