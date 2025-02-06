import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core'

import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'

import RouteLayer from '@arcgis/core/layers/RouteLayer'

import FeatureSet from '@arcgis/core/rest/support/FeatureSet'
import * as route from '@arcgis/core/rest/route'
import RouteParameters from '@arcgis/core/rest/support/RouteParameters.js'

import SimpleMarkerSymbol from '@arcgis/core/symbols/SimpleMarkerSymbol'
import SimpleLineSymbol from '@arcgis/core/symbols/SimpleLineSymbol'
import Graphic from '@arcgis/core/Graphic'

@Component({
  selector: 'route-stops',
  templateUrl: './route.component.html',
  styleUrls: ['./route.component.scss'],
})
export class RouteComponent implements AfterViewInit {
  // @Input() Directions: any
  // @Output() ISstartRoute = new EventEmitter<any>() // ส่งออก
  // @Output() selectedPath = new EventEmitter<any>() // ส่งออก
  // @Output() isClear = new EventEmitter<boolean>() // ส่งออก
  @ViewChild('mapPanel', { static: true }) mapPanel!: ElementRef
  view!: MapView
  // Directions: any
  directionsPaths: String[] = []
  selectedDirection: number = -1
  sandiegoRoute = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route'
  sandiegoRouteLayer = new RouteLayer({
    url: this.sandiegoRoute,
  })
  svgPath =
    'M213.285,0h-0.608C139.114,0,79.268,59.826,79.268,133.361c0,48.202,21.952,111.817,65.246,189.081 c32.098,57.281,64.646,101.152,64.972,101.588c0.906,1.217,2.334,1.934,3.847,1.934c0.043,0,0.087,0,0.13-0.002 c1.561-0.043,3.002-0.842,3.868-2.143c0.321-0.486,32.637-49.287,64.517-108.976c43.03-80.563,64.848-141.624,64.848-181.482 C346.693,59.825,286.846,0,213.285,0z M274.865,136.62c0,34.124-27.761,61.884-61.885,61.884 c-34.123,0-61.884-27.761-61.884-61.884s27.761-61.884,61.884-61.884C247.104,74.736,274.865,102.497,274.865,136.62z'
  clickedPoints = []
  Stops: any

  // ngOnChanges() {
  //   if (this.Directions) {
  //     console.log('ผลลัพธ์การคำนวณเส้นทาง:', this.Directions)
  //   }
  // }

  ngAfterViewInit(): void {
    const map = new Map({
      basemap: 'topo-vector',
    })

    this.view = new MapView({
      container: this.mapPanel.nativeElement,
      map: map,
      center: [-117.161087, 32.715736],
      zoom: 16,
    })

    this.view.when(() => {
      this.view.on('click', (event) => Routes(event)) // Route

      const Routes = (event) => {
        const pointSymbol =
          this.clickedPoints.length === 0
            ? // สร้างสัญลักษณ์หมุด
              // new PictureMarkerSymbol({
              //   url: 'https://www.svgrepo.com/show/69593/location-pin.svg', // URL รูป SVG
              //   width: '100px',
              //   height: '100px',
              // })
              new SimpleMarkerSymbol({
                path: this.svgPath,
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

          route
            .solve(this.sandiegoRoute, routeParams)
            .then((response) => {
              this.Stops = response
            })
            .catch((error) => {
              alert('Please route only SanDiego')
              this.clearAll()
            })
        }
      }
    })
  }

  startRoute() {
    this.directionsPaths = []
    this.selectedDirection = -1

    const features = this.Stops.routeResults[0].directions.features
    if (features && features.length > 0) {
      features.map((response) => {
        this.directionsPaths.push(response.attributes.text)
        const routeGraphic = new Graphic({
          geometry: response.geometry,
          symbol: new SimpleLineSymbol({
            color: [0, 0, 255],
            width: 1,
          }),
        })

        this.view.graphics.add(routeGraphic)
      })
      // this.ISstartRoute.emit(true)
    }
  }

  // startRoute(event) {
  //   if (event) {
  //     const features = this.Stops.routeResults[0].directions.features
  //     if (features && features.length > 0) {
  //       features.map((response) => {
  //         const routeGraphic = new Graphic({
  //           geometry: response.geometry,
  //           symbol: new SimpleLineSymbol({
  //             color: [0, 0, 255],
  //             width: 1,
  //           }),
  //         })

  //         this.view.graphics.add(routeGraphic)
  //       })
  //     }
  //   }
  // }

  selectDirection(index: number) {
    this.selectedDirection = index
    // this.selectedPath.emit(index)
    this.zoomRoute(index)
  }

  clearAll() {
    this.Stops = null
    this.directionsPaths = []
    this.selectedDirection = -1
    // this.isClear.emit(true)

    this.clickedPoints = []
    this.view.graphics.removeAll()
    this.view.goTo({
      center: [-117.161087, 32.715736],
      zoom: 16,
    })
  }

  zoomRoute(event) {
    const focus = this.Stops.routeResults[0].directions.features[event].geometry
    this.view.goTo({
      target: focus.extent.expand(1),
    })
  }
}
