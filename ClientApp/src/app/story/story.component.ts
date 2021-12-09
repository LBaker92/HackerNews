import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { StoryData } from '../models/story-data';
import { StoryService } from '../services/story.service';

@Component({
  selector: 'app-story',
  templateUrl: './story.component.html',
  styleUrls: ['./story.component.scss'],
})
export class StoryComponent implements OnInit {
  dataSource!: StoryData;
  displayedColumns = ['stories'];
  totalStories = 0;
  pageIndex = 0;
  pageSize = 30;
  searchText = '';
  isInitLoadCompleted = false;
  isLoading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private storyService: StoryService,
    private snackBar: MatSnackBar
  ) {
    this.storyService
      .getStories(this.pageIndex, this.pageSize, this.searchText)
      .subscribe(
        (response: StoryData) => {
          this.dataSource = response;
          this.isInitLoadCompleted = true;
          this.isLoading = false;
        },
        (error: StoryData) => {
          error.errors.forEach((error) => {
            this.snackBar.open(error, 'Close', { duration: 5000 });
          });
        }
      );
  }

  ngOnInit(): void {}

  onSearchFieldChange(): void {
    this.isLoading = true;

    this.pageIndex = 0;
    this.paginator.pageIndex = 0;

    this.storyService
      .getStories(this.pageIndex, this.pageSize, this.searchText)
      .subscribe(
        (response: StoryData) => {
          this.dataSource = response;
          this.isInitLoadCompleted = true;
          this.isLoading = false;
        },
        (error: StoryData) => {
          error.errors.forEach((error) => {
            this.snackBar.open(error, 'Close', { duration: 5000 });
          });
        }
      );
  }

  onPageChange(event: PageEvent): void {
    this.isLoading = true;

    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;

    this.storyService
      .getStories(this.pageIndex, this.pageSize, this.searchText)
      .subscribe(
        (response: StoryData) => {
          this.dataSource = response;
          this.isInitLoadCompleted = true;
          this.isLoading = false;
        },
        (error: StoryData) => {
          error.errors.forEach((error) => {
            this.snackBar.open(error, 'Close', { duration: 5000 });
          });
        }
      );
  }
}
