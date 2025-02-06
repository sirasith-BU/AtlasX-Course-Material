import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core'

import { CustomPoint } from './custom-point.model'

import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'

import MapImageLayer from '@arcgis/core/layers/MapImageLayer'
import FeatureLayer from '@arcgis/core/layers/FeatureLayer.js'
import TileLayer from '@arcgis/core/layers/TileLayer'
import RouteLayer from '@arcgis/core/layers/RouteLayer'
import GraphicLayer from '@arcgis/core/layers/GraphicsLayer'

import FeatureSet from '@arcgis/core/rest/support/FeatureSet'
import { identify } from '@arcgis/core/rest/identify'
import IdentifyParameters from '@arcgis/core/rest/support/IdentifyParameters'
import * as closestFacility from '@arcgis/core/rest/closestFacility'
import ClosestFacilityParameters from '@arcgis/core/rest/support/ClosestFacilityParameters'
import * as route from '@arcgis/core/rest/route'
import RouteParameters from '@arcgis/core/rest/support/RouteParameters.js'

import Point from '@arcgis/core/geometry/Point'
import SimpleMarkerSymbol from '@arcgis/core/symbols/SimpleMarkerSymbol'
import SimpleFillSymbol from '@arcgis/core/symbols/SimpleFillSymbol'
import SimpleLineSymbol from '@arcgis/core/symbols/SimpleLineSymbol'
import Graphic from '@arcgis/core/Graphic'
import * as geometryEngine from '@arcgis/core/geometry/geometryEngine.js'
import PictureMarkerSymbol from '@arcgis/core/symbols/PictureMarkerSymbol'

import Sketch from '@arcgis/core/widgets/Sketch'
import LayerList from '@arcgis/core/widgets/LayerList.js'
import Swipe from '@arcgis/core/widgets/Swipe.js'

@Component({
  selector: 'app-locator',
  templateUrl: './locator.component.html',
  styleUrls: ['./locator.component.scss'],
})
export class LocatorComponent implements AfterViewInit {
  @ViewChild('mapPanel', { static: true }) mapPanel!: ElementRef

  // ค่าเริ่มต้นของ lat,long ของ map
  lat: number = 40.01512636
  long: number = -99.36306483

  // Sandiego
  // lat: number = 32.715736
  // long: number = -117.161087

  view!: MapView
  clickPoint: CustomPoint
  Routes: any
  Stops: any
  clickedPoints = []

  ngAfterViewInit(): void {
    // Map เป็นแบบ topo-vector
    const map = new Map({
      basemap: 'topo-vector',
    })

    const censusURL = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/'
    const usaURL = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/'
    const usaURL2 = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2'
    const usaURL0 = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0'
    const SanDiegoCF =
      'https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility'
    const sandiegoRoute =
      'https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route'
    const Wildfire = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire/FeatureServer/2'
    const world_ocean_baseURL =
      'https://services.arcgisonline.com/arcgis/rest/services/Ocean/World_Ocean_Base/MapServer'
    const world_street_mapURL = 'https://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer'

    const CensusLayers = new MapImageLayer({
      url: censusURL,
    })
    const USALayers = new MapImageLayer({
      url: usaURL,
    })
    const usaFeatureLayer2 = new FeatureLayer({
      url: usaURL2,
    })
    const usaFeatureLayer0 = new FeatureLayer({
      url: usaURL0,
    })
    const WildfireFeatureLayer2 = new FeatureLayer({
      url: Wildfire,
    })
    const graphicsLayer = new GraphicLayer()
    const sandiegoRouteLayer = new RouteLayer({
      url: sandiegoRoute,
    })
    const OceanLayers = new TileLayer({
      url: world_ocean_baseURL,
    })
    const world_Street = new TileLayer({
      url: world_street_mapURL,
    })

    // map.add(USALayers)
    // map.add(usaFeatureLayer2) // Query Task
    // map.add(usaFeatureLayer0) // Closet-Facility
    // map.add(WildfireFeatureLayer2) // Sketch
    // map.add(graphicsLayer) // Sketch
    // map.add(sandiegoRouteLayer) // Route
    // map.add(OceanLayers) // Layer List, Swipe
    // map.add(world_Street) // Swipe
    // map.add(CensusLayers) // Layer List

    // Property ของ MapView
    this.view = new MapView({
      container: this.mapPanel.nativeElement,
      map: map,
      center: [this.long, this.lat],
      zoom: 5,
      // zoom: 16,
    })

    this.view.when(() => {
      console.log('MapView is ready!')
      // สร้าง Point เริ่มต้นตอนเข้าหน้าเว็บ
      // this.addPoint(this.lat, this.long)

      // Click event MapView
      this.view.on("click", (event) => sendMapPoint(event)) // Send Longitdue,Latitude
      // this.view.on('click', (event) => executeIdentify(event)) // Identify Task
      // this.view.on('click', (event) => closetFacility(event)) // Closest Facility
      // this.view.on('click', (event) => Routes(event)) // Route

      // Function ที่รองรับเมื่อเกิดการClick
      const executeIdentify = (event) => {
        // IdentifyParameters
        const params = new IdentifyParameters()
        params.tolerance = 3
        params.layerIds = [3] //Layers ที่เลือก
        params.layerOption = 'top'
        params.width = this.view.width
        params.height = this.view.height
        params.returnGeometry = true // return geometry
        params.geometry = event.mapPoint
        params.mapExtent = this.view.extent

        identify(censusURL, params).then((response) => {
          const results = response.results

          // Map ค่าใน results แล้ว return ค่ามาใส่ใน features
          const features = results.map((result) => {
            const feature = result.feature
            const layerName = result.layerName

            feature.attributes.layerName = layerName

            // ถ้าที่Clickไป layerName = states ให้แสดงตามค่าใน popupTemplate
            if (layerName === 'states') {
              const stateName = feature.attributes.STATE_NAME.toUpperCase()
              const population = new Intl.NumberFormat().format(feature.attributes.POP2007)
              const area = parseFloat(feature.attributes.Shape_Area).toFixed(2)

              feature.popupTemplate = {
                title: `${stateName}`,
                content: `<b>Population (2007):</b> ${population}<br><b>Area:</b> ${area}`,
              }

              // addPolygon Highlight ตามค่า geometry ที่ใส่เข้าไป
              this.addPolygon(feature.geometry)
            }
            return feature
          })
          showPopup(features, event.mapPoint)
        })
      }
      const sendMapPoint = (event) => {
        const customPoint = new CustomPoint(event.mapPoint.latitude, event.mapPoint.longitude)
        this.clickPoint = customPoint
      }
      const closetFacility = (event) => {
        let Buffer = this.addIncident(event.mapPoint)

        const query = usaFeatureLayer0.createQuery()
        query.geometry = Buffer
        query.spatialRelationship = 'intersects'
        query.returnGeometry = true
        query.outFields = ['*']

        usaFeatureLayer0.queryFeatures(query).then((result) => {
          const facility = result.features.map((feature) => {
            const marker = new SimpleMarkerSymbol({
              color: [255, 128, 0, 0.8],
              style: 'square',
              size: '10px',
              outline: {
                color: [255, 255, 255],
                width: 1,
              },
            })

            const pointGraphic = new Graphic({
              geometry: feature.geometry,
              symbol: marker,
              attributes: {
                name: feature.attributes.areaname,
              },
            })

            this.view.graphics.add(pointGraphic)
            return pointGraphic
          })

          const incident = new Graphic({
            geometry: event.mapPoint,
            symbol: new SimpleMarkerSymbol({
              color: [255, 128, 0],
              size: '12px',
              outline: {
                color: [255, 255, 255],
                width: 1,
              },
            }),
          })
          this.view.graphics.add(incident)

          closestFacility
            .solve(
              SanDiegoCF,
              new ClosestFacilityParameters({
                incidents: new FeatureSet({
                  features: [incident],
                }),
                facilities: new FeatureSet({
                  features: facility,
                }),
                returnRoutes: true,
                defaultTargetFacilityCount: 10,
              })
            )
            .then((response) => {
              this.Routes = response
              console.log(this.Routes)
              response.routes.features.map((route) => {
                const routeGraphic = new Graphic({
                  geometry: route.geometry,
                  symbol: new SimpleLineSymbol({
                    color: [0, 191, 255, 0.8],
                    width: 3,
                  }),
                })
                this.view.graphics.add(routeGraphic)
              })
            })
        })
      }

      // Sketch & Apply Edits
      let sketch = new Sketch({
        layer: graphicsLayer,
        view: this.view,
        availableCreateTools: ['polygon'],
      })
      // this.view.ui.add(sketch, 'top-right')

      let currentObjectId: number
      sketch.on('create', (event) => {
        if (event.state === 'complete') {
          const addFeature = new Graphic({
            geometry: event.graphic.geometry,
            symbol: new SimpleFillSymbol({
              color: [255, 128, 0, 0.5], // Orange Opacity=0.5
              outline: {
                color: [255, 255, 255], // White
                width: 2,
              },
            }),
            attributes: {
              symbolid: 0,
              description: 'Add by Chaky',
            },
          })

          WildfireFeatureLayer2.applyEdits({
            addFeatures: [addFeature],
          }).then((response) => {
            // currentObjectId = response.addFeatureResults[0].objectId
            // console.log(response)
          })
        }
      })
      sketch.on('update', (event) => {
        if (event.state === 'start') {
          const selectedGeometry = event.graphics[0].geometry

          const query = WildfireFeatureLayer2.createQuery()
          query.geometry = selectedGeometry
          query.spatialRelationship = 'intersects'
          query.returnGeometry = true
          query.outFields = ['OBJECTID']

          WildfireFeatureLayer2.queryFeatures(query).then((result) => {
            if (result.features.length > 0) {
              // currentObjectId = result.features[0].attributes.objectid
              const matchingFeature = result.features.find((feature) =>
                geometryEngine.equals(feature.geometry, selectedGeometry)
              )
              if (matchingFeature) {
                currentObjectId = matchingFeature.attributes.objectid
              }
            }
          })
        }

        if (event.state === 'complete') {
          const updateFeature = new Graphic({
            geometry: event.graphics[0].geometry,
            symbol: new SimpleFillSymbol({
              color: [255, 128, 0, 0.5], // Orange Opacity=0.5
              outline: {
                color: [255, 255, 255], // White
                width: 2,
              },
            }),
            attributes: {
              objectid: currentObjectId,
              description: 'Update by Chaky',
            },
          })

          WildfireFeatureLayer2.applyEdits({
            updateFeatures: [updateFeature],
          }).then((response) => {
            console.log(response)
          })
        }
      })
      sketch.on('delete', () => {
        const deleteFeature = [
          {
            objectId: currentObjectId,
          },
        ]
        WildfireFeatureLayer2.applyEdits({
          deleteFeatures: deleteFeature,
        }).then((response) => {
          console.log(response)
        })
      })

      const Routes = (event) => {
        let svgPath =
          'M213.285,0h-0.608C139.114,0,79.268,59.826,79.268,133.361c0,48.202,21.952,111.817,65.246,189.081 c32.098,57.281,64.646,101.152,64.972,101.588c0.906,1.217,2.334,1.934,3.847,1.934c0.043,0,0.087,0,0.13-0.002 c1.561-0.043,3.002-0.842,3.868-2.143c0.321-0.486,32.637-49.287,64.517-108.976c43.03-80.563,64.848-141.624,64.848-181.482 C346.693,59.825,286.846,0,213.285,0z M274.865,136.62c0,34.124-27.761,61.884-61.885,61.884 c-34.123,0-61.884-27.761-61.884-61.884s27.761-61.884,61.884-61.884C247.104,74.736,274.865,102.497,274.865,136.62z'

        const pointSymbol =
          this.clickedPoints.length === 0
            ? // สร้างสัญลักษณ์หมุด
              // new PictureMarkerSymbol({
              //   url: 'https://www.svgrepo.com/show/69593/location-pin.svg', // URL รูป SVG
              //   width: '100px',
              //   height: '100px',
              // })
              new SimpleMarkerSymbol({
                path: svgPath,
                color: [255, 0, 0, 0.8], // สีของหมุด
                size: 30, // ขนาดหมุด
                outline: {
                  color: [0, 0, 0, 0.5],
                  width: 1,
                },
              })
            : new SimpleMarkerSymbol({
                // จุดต่อไป
                color: [255, 128, 0], // Orange
                outline: {
                  color: [255, 255, 255], // White
                  width: 1,
                },
              })

        const pointGraphic = new Graphic({
          geometry: event.mapPoint,
          symbol: pointSymbol,
        })

        this.view.graphics.add(pointGraphic)
        this.clickedPoints.push(pointGraphic)

        if (this.clickedPoints.length > 1) {
          const stops = new FeatureSet({
            features: this.clickedPoints,
          })

          const routeParams = new RouteParameters({
            stops: stops,
            returnRoutes: true,
            returnDirections: true,
          })

          route.solve(sandiegoRoute, routeParams).then((response) => {
            this.Stops = response
          })
        }
      }

      const layerList = new LayerList({
        view: this.view,
        // container: this.mapPanel.nativeElement,
      })
      layerList.dragEnabled = true
      // this.view.ui.add(layerList, {
      //   position: 'bottom-left',
      // })

      let swipe = new Swipe({
        view: this.view,
        leadingLayers: [OceanLayers],
        trailingLayers: [world_Street],
        direction: 'horizontal',
        position: 50,
      })
      // this.view.ui.add(swipe)

      // showPopup ตาม features และตำแหน่งที่Click
      const showPopup = (features, mapPoint) => {
        if (features.length > 0) {
          this.view.popup.open({
            features: features,
            location: mapPoint,
          })
        }
      }
    })
  }

  addPoint(latitude: number, longitude: number) {
    // Point รับค่าเป็น longitude, latitude
    const point = new Point({
      longitude: longitude,
      latitude: latitude,
    })

    // Point SimpleMarkerSymbol
    const marker = new SimpleMarkerSymbol({
      color: [255, 128, 0, 0.5], // Orange Opacity=0.5
      outline: {
        color: [255, 255, 255], // White
        width: 1,
      },
    })

    const pointGraphic = new Graphic({
      geometry: point,
      symbol: marker,
    })

    this.view.graphics.removeAll()
    this.view.graphics.add(pointGraphic)
  }

  addPolygon(geometry) {
    // geometry ที่รับเข้ามาจะมี rings ซึ่งจะเป็นพิกัดของ Polygon ที่จะสร้าง
    const marker = new SimpleFillSymbol({
      color: [255, 128, 0, 0.5], // Orange Opacity=0.5
      outline: {
        color: [255, 255, 255], // White
        width: 2,
      },
    })

    const polygonGraphic = new Graphic({
      geometry: geometry,
      symbol: marker,
    })

    this.view.graphics.removeAll()
    this.view.graphics.add(polygonGraphic)
  }

  addIncident(geometry) {
    // buffer 20 กิโลเมตร
    const ptBuff = geometryEngine.buffer(geometry, 20, 'kilometers')

    // ตรวจสอบว่า ptBuff เป็น Array หรือไม่
    const Buffer = Array.isArray(ptBuff) ? ptBuff[0] : ptBuff

    const marker = new SimpleFillSymbol({
      color: [255, 128, 0, 0.5], // Orange
      outline: {
        color: [255, 255, 255], // White
        width: 1,
      },
    })

    const bufferGraphic = new Graphic({
      geometry: Buffer, // ใช้ geometry ของ buffer
      symbol: marker,
    })

    this.view.graphics.removeAll()
    this.view.graphics.add(bufferGraphic)

    // Zoom and Pan to Polygon
    this.view.goTo({
      target: Buffer.extent.expand(1),
    })
    return Buffer
  }

  // ได้รับข้อมูลาจาก LocatorFormComponent (locate)="showLocate($event)"
  showLocate(event) {
    if (event) {
      let lat = event.latitude
      let long = event.longitude

      // .goTo เพื่อไปยังตำแหน่งตาม lat,long ที่ได้มา
      this.view
        .goTo({
          center: [long, lat],
          zoom: 10,
        })
        .then(() => {
          // พร้อมเพิ่ม Point ตาม lat,long ที่ได้มา
          this.addPoint(lat, long)
        })
    }
  }

  showPolygon(polygon) {
    const marker = new SimpleFillSymbol({
      color: [255, 128, 0, 0.5], // Orange Opacity=0.5
      outline: {
        color: [255, 255, 255], // White
        width: 2,
      },
    })

    const polygonGraphic = new Graphic({
      geometry: polygon,
      symbol: marker,
    })

    this.view.graphics.removeAll()
    this.view.graphics.add(polygonGraphic)

    // Zoom and Pan to Polygon
    this.view.goTo({
      target: polygon.extent.expand(1.5),
    })
  }

  private routeGraphic: Graphic
  showSelectedRoutes(geometry) {
    // ลบเส้นก่อนหน้า
    if (this.routeGraphic) {
      this.view.graphics.remove(this.routeGraphic)
    }
    this.routeGraphic = new Graphic({
      geometry: geometry,
      symbol: new SimpleLineSymbol({
        color: [0, 0, 0],
        width: 3,
      }),
    })

    this.view.graphics.add(this.routeGraphic)

    // Zoom and Pan to Polygon
    this.view.goTo({
      target: geometry.extent.expand(1.5),
    })
  }

  startRoute(event) {
    if (event) {
      const features = this.Stops.routeResults[0].directions.features
      if (features && features.length > 0) {
        features.map((response) => {
          const routeGraphic = new Graphic({
            geometry: response.geometry,
            symbol: new SimpleLineSymbol({
              color: [0, 0, 255],
              width: 1,
            }),
          })

          this.view.graphics.add(routeGraphic)
        })
      }
    }
  }

  clearAll(event) {
    if (event) {
      this.clickedPoints = []
      this.view.graphics.removeAll()
      this.view.goTo({
        center: [-117.161087, 32.715736],
        zoom: 16,
      })
    }
  }

  zoomRoute(event) {
    const focus = this.Stops.routeResults[0].directions.features[event].geometry
    this.view.goTo({
      target: focus.extent.expand(1),
    })
  }
}
