import { Component } from '@angular/core'

interface Comment {
  name: string
  comment: string
  timestamp: string
}

@Component({
  selector: 'app-assignment1',
  templateUrl: './assignment1-3.component.html',
  // styleUrls: ['./assignment1.component.scss'],
})
export class Assignment1Component {
  comments: Comment[] = []
  currentComment: Comment = this.getEmptyComment()
  editIndex: number = -1

  private getEmptyComment(): Comment {
    return {
      name: '',
      comment: '',
      timestamp: '',
    }
  }

  private getCurrentTimestamp(): string {
    const now = new Date()
    return now.toLocaleString()
  }

  onSubmit() {
    if (this.editIndex === -1) {
      // Add new comment
      this.comments.push({
        ...this.currentComment,
        timestamp: this.getCurrentTimestamp(),
      })
    } else {
      // Update existing comment
      this.comments[this.editIndex] = {
        ...this.currentComment,
        timestamp: this.getCurrentTimestamp(),
      }
      this.editIndex = -1
    }

    // Reset form
    this.currentComment = this.getEmptyComment()
  }

  editComment(index: number) {
    this.editIndex = index
    this.currentComment = { ...this.comments[index] }
  }

  deleteComment(index: number) {
    this.comments.splice(index, 1)
  }
}
