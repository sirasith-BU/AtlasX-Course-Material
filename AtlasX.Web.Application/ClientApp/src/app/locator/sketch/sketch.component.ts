import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core'

import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'

import FeatureLayer from '@arcgis/core/layers/FeatureLayer.js'
import GraphicLayer from '@arcgis/core/layers/GraphicsLayer'

import SimpleFillSymbol from '@arcgis/core/symbols/SimpleFillSymbol'
import Graphic from '@arcgis/core/Graphic'
import * as geometryEngine from '@arcgis/core/geometry/geometryEngine.js'

import Sketch from '@arcgis/core/widgets/Sketch'

@Component({
  selector: 'sketch-applyEdits',
  templateUrl: './sketch.component.html',
  styleUrls: ['./sketch.component.scss'],
})
export class SketchComponent implements AfterViewInit {
  @ViewChild('mapPanel', { static: true }) mapPanel!: ElementRef
  view!: MapView
  Wildfire = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire/FeatureServer/2'
  WildfireFeatureLayer2 = new FeatureLayer({
    url: this.Wildfire,
  })
  graphicsLayer = new GraphicLayer()

  ngAfterViewInit(): void {
    const map = new Map({
      basemap: 'topo-vector',
    })

    this.view = new MapView({
      container: this.mapPanel.nativeElement,
      map: map,
      center: [-99.36306483, 40.01512636],
      zoom: 4,
    })

    map.add(this.WildfireFeatureLayer2)
    map.add(this.graphicsLayer)

    this.view.when(() => {
      // Sketch & Apply Edits
      let sketch = new Sketch({
        layer: this.graphicsLayer,
        view: this.view,
        availableCreateTools: ['polygon'],
      })
      this.view.ui.add(sketch, 'top-right')

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

          this.WildfireFeatureLayer2.applyEdits({
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

          const query = this.WildfireFeatureLayer2.createQuery()
          query.geometry = selectedGeometry
          query.spatialRelationship = 'intersects'
          query.returnGeometry = true
          query.outFields = ['OBJECTID']

          this.WildfireFeatureLayer2.queryFeatures(query).then((result) => {
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

          this.WildfireFeatureLayer2.applyEdits({
            updateFeatures: [updateFeature],
          }).then((response) => {
            // console.log(response)
          })
        }
      })
      sketch.on('delete', () => {
        const deleteFeature = [
          {
            objectId: currentObjectId,
          },
        ]
        this.WildfireFeatureLayer2.applyEdits({
          deleteFeatures: deleteFeature,
        }).then((response) => {
          //   console.log(response)
        })
      })
    })
  }
}
