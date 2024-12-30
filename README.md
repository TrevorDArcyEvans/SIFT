# Pure C# Implementation of SIFT (Scaled Invariant Feature Transform)

Find local features using [SIFT](https://en.wikipedia.org/wiki/Scale-invariant_feature_transform)

![baseline](Bikesgray.jpg)

![left](assets/original_left.jpg)
![right](assets/original_right.jpg)

![keypoints left](assets/keypoints_left.jpg)
![keypoints right](assets/keypoints_right.jpg)

![corrected](assets/corrected.jpg)
![corrected right](assets/corrected_right.jpg)

Based on code from:<br/>
  * https://github.com/karashiiro/SIFT
  * https://github.com/karashiiro/GradientDotNet


## Prerequisites

* .NET9 SDK


## Getting started

```bash
git clone https://github.com/TrevorDArcyEvans/SIFT.git
cd SIFT
dotnet restore
dotnet build
dotnet run
```


