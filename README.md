# Pure C# Implementation of SIFT (Scaled Invariant Feature Transform)

Find local features using [SIFT](https://en.wikipedia.org/wiki/Scale-invariant_feature_transform)

![baseline](SIFT.Demo/Bikesgray.jpg)<br/>
  Baseline = image from https://upload.wikimedia.org/wikipedia/commons/3/3f/Bikesgray.jpg
<br/>

![left](assets/original_left.jpg)<br/>
  Left = Baseline + cropped
<br/>

![right](assets/original_right.jpg)<br/>
  Right = Baseline cropped + rotated
<br/>

![keypoints left](assets/keypoints_left.jpg)<br/>
  Keypoints Left
<br/>

![keypoints right](assets/keypoints_right.jpg)<br/>
  Keypoints Right
<br/>

![corrected right](assets/corrected_right.jpg)<br/>
  Corrected Right = Keypoints Right + rotated + scaled
<br/>

![corrected](assets/corrected.jpg)<br/>
Corrected = Corrected Right + merged + Keypoints Left
<br/>

Based on code from:<br/>

* https://github.com/karashiiro/SIFT
* https://github.com/karashiiro/GradientDotNet

## Prerequisites

* .NET9 SDK

## Getting started

```bash
git clone --recurse-submodules https://github.com/TrevorDArcyEvans/SIFT.git
cd SIFT
dotnet restore
dotnet build
cd SIFT.Demo
dotnet run

# examine contents of current directory
```

## Using SIFT to recognise features

![](assets/baseline-phone_cam_15-vs-phone_cam_16.png)
![](assets/comparison-phone_cam_15-vs-phone_cam_16.jpg)
