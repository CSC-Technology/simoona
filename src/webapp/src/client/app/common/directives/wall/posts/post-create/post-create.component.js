(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceWallPostCreate', {
            bindings: {
                onCreatePost: '&',
                wallType: '=',
                isWallModule: '='
            },
            templateUrl: 'app/common/directives/wall/posts/post-create/post-create.html',
            controller: wallPostCreateController,
            controllerAs: 'vm'
        });

    wallPostCreateController.$inject = [
        '$scope',
        'imageValidationSettings',
        'shroomsFileUploader',
        'notifySrv',
        'pictureRepository',
        'wallImageConfig',
        'wallSettings',
        'errorHandler',
        'dataHandler',
        'wallPostRepository',
        'mentionService',
        '$translate'
    ];

    function wallPostCreateController($scope, imageValidationSettings, shroomsFileUploader,
        notifySrv, pictureRepository, wallImageConfig, wallSettings, errorHandler, dataHandler, wallPostRepository, mentionService, translate) {
        /*jshint validthis: true */
        var vm = this;

        vm.submitPost = submitPost;
        vm.showSubmit = showSubmit;
        vm.attachImage = attachImage;
        vm.isSubmittable = isSubmittable;
        vm.handleErrorMessage = handleErrorMessage;

        vm.postForm = {};
        vm.attachedFiles = [];
        vm.isFormEnabled = true;
        vm.showSubmitButton = false;
        vm.maxLength = wallSettings.postMaxLength;
        vm.thumbHeight = wallImageConfig.thumbHeight;

        vm.mentions = mentionService.mentions();

        init();
        //////////

        function init() {
            vm.startConversation = translate.instant('wall.startConversation');
        }

        function isSubmittable() {
            if (!!vm.postForm.messageBody) {
                return !!vm.postForm.messageBody.trim().length && vm.isFormEnabled;
            }
        }

        function submitPost() {
            if (isSubmittable()) {
                vm.isFormEnabled = false;

                if (vm.attachedFiles.length) {
                    pictureRepository.upload(vm.attachedFiles).then(function(promise) {
                        handleFormSubmit(promise.data);
                    }, vm.handleErrorMessage);
                } else {
                    handleFormSubmit();
                }
            }
        }

        function handleFormSubmit(pictureId) {
            vm.postForm.images = pictureId ? [pictureId] : null;
            vm.postForm.mentionedUserIds = vm.mentions.getValidatedMentions(vm.postForm.messageBody);

            vm.onCreatePost({ post: vm.postForm });

            clearPost();
        }

        function handleErrorMessage(errorResponse) {
            vm.isFormEnabled = true;

            errorHandler.handleErrorMessage(errorResponse);
        }

        function showSubmit() {
            vm.showSubmitButton = true;
        }

        function clearPost() {
            vm.postForm = {};
            vm.attachedFiles = [];
            vm.imageSource = null;
            vm.showSubmitButton = false;
            vm.isFormEnabled = true;
        }

        function attachImage(input) {
            var options = {
                canvas: true
            };

            if (input.value) {
                if (shroomsFileUploader.validate(input.files, imageValidationSettings, showUploadAlert)) {
                    vm.attachedFiles = shroomsFileUploader.fileListToArray(input.files);

                    var displayImg = function(img) {
                        $scope.$apply(function($scope) {
                            var fileName = vm.attachedFiles[0].name;

                            vm.imageSource = img.toDataURL(vm.attachedFiles[0].type);
                            if (vm.attachedFiles[0].type !== 'image/gif') {
                                vm.attachedFiles[0] = dataHandler.dataURItoBlob(vm.imageSource, vm.attachedFiles[0].type);
                                vm.attachedFiles[0].name = fileName;
                            }
                        });
                    };

                    loadImage.parseMetaData(vm.attachedFiles[0], function (data) {
                        if (data.exif) {
                            options.orientation = data.exif.get('Orientation');
                        }

                        loadImage(vm.attachedFiles[0], displayImg, options);
                    });
                }
            }
            input.value = '';
        }

        function showUploadAlert(status) {
            if (status === 'sizeError') {
                notifySrv.error('wall.imageSizeExceeded');
            } else if (status === 'typeError') {
                notifySrv.error('wall.imageInvalidType');
            }
            $scope.$apply();
        }
    }
}());
