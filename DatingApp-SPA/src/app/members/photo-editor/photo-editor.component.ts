import { Component, OnInit, Input, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { constructor } from 'q';
import { get } from 'http';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  @Output() mainPhotoChanged = new EventEmitter();
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;

  constructor(private authService: AuthService, private userService: UserService, private alertify: AlertifyService) { }

  get userId(): number {
    return this.authService.decodedToken.nameid;
  }

  ngOnInit() {
    this.initUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/' + this.userId + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => { file.withCredentials = false; };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        const photo = {
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain
        };
        this.photos.push(photo);
      }
    };
  }

  setPhotoMain(photo: Photo) {
    this.photos.map((p) => { p.isMain = false; });
    photo.isMain = true;
    this.userService.setMainPhoto(this.userId, photo.id).subscribe(() => {
      this.mainPhotoChanged.emit(photo);
      this.authService.changeMemberPhoto(photo.url);
      this.alertify.success('Success');
    }, error => {
      this.alertify.error(error);
    });
  }

  delete(photo: Photo) {
    this.alertify.confirm('Are you sure?', () => {
      this.userService.deletePhoto(this.userId, photo.id).subscribe(() => {
        this.photos.splice(this.photos.indexOf(photo), 1);
        this.alertify.success('Success');
      }, error => {
        this.alertify.error(error);
      });
    });
  }

}
