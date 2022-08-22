(function () {
    'use strict';

    angular
        .module('simoonaApp.ServiceRequest')
        .controller('newRequestModalController', newRequestModalController);

    newRequestModalController.$inject = [
        '$scope',
        '$uibModalInstance',
        'serviceRequestRepository',
        'notifySrv',
        'kudosifySettings',
        'lodash',
        'imageValidationSettings',
        'shroomsFileUploader',
        'pictureRepository',
        'dataHandler',
    ];

    function newRequestModalController(
        $scope,
        $uibModalInstance,
        serviceRequestRepository,
        notifySrv,
        kudosifySettings,
        lodash,
        imageValidationSettings,
        shroomsFileUploader,
        pictureRepository,
        dataHandler
    ) {
        $scope.categories = '';
        $scope.priorities = '';
        $scope.statuses = '';
        $scope.isDisabled = false;
        $scope.maxKudosMinus = kudosifySettings.maxMinus;
        $scope.isAssignee = $scope.serviceRequest.isCloseable;

        $scope.attachedFiles = [];
        $scope.attachImage = attachImage;

        $scope.isAdminOrAssignee = isAdminOrAssignee;
        $scope.isEditing = isEditing;
        $scope.isServiceRequestDone = isServiceRequestDone;


        init();

        ////////////

        function init() {
            getShopItems();
            getKudosCategories();
            getPriorities();
            getStatuses();
        }

        function getShopItems() {
            serviceRequestRepository.getShopItems().then(function (response) {
                $scope.kudosShopItems = response;
            });
        }

        function getKudosCategories() {
            serviceRequestRepository.getCategories().then(function (response) {
                $scope.categories = response;
                if (!$scope.edit && $scope.setCategoryToKudos) {
                    angular.forEach($scope.categories, function (category) {
                        if (category.name === 'Kudos') {
                            $scope.serviceRequest.serviceRequestCategory =
                                category;
                        }
                    });
                } else if (!$scope.edit) {
                    $scope.serviceRequest.serviceRequestCategory =
                        $scope.categories[0];
                } else {
                    $scope.serviceRequest.serviceRequestCategory = lodash.find(
                        $scope.categories,
                        {
                            name: $scope.serviceRequest.categoryName,
                        }
                    );
                }
            });
        }

        function getPriorities() {
            serviceRequestRepository.getPriorities().then(function (response) {
                $scope.priorities = response;
                if (!$scope.edit) {
                    $scope.serviceRequest.priority = $scope.priorities[1];
                }
            });
        }

        function getStatuses() {
            serviceRequestRepository.getStatuses().then(function (response) {
                $scope.statuses = response;
                if (!$scope.edit) {
                    $scope.serviceRequest.status = $scope.statuses[0];
                }
            });
        }

        $scope.getRequestComments = function () {
            serviceRequestRepository
                .getComments($scope.serviceRequest.id)
                .then(function (response) {
                    $scope.comments = response;
                });
        };

        if ($scope.edit) {
            $scope.getRequestComments();
            $scope.initialModel = angular.copy($scope.serviceRequest);
        }

        $scope.submitRequestWithComment = function () {
            if (!$scope.newComment.content) {
                $scope.submitServiceRequest();
                $scope.isDisabled = true;
            } else {
                $scope.newComment.ServiceRequestId = $scope.serviceRequest.id;
                $scope.isDisabled = true;
                serviceRequestRepository
                    .postComment($scope.newComment)
                    .then($scope.submitServiceRequest, onError);
            }
        };

        $scope.submitServiceRequest = function () {
            if (isEditing() && !isServiceRequestModelUpdated()) {
                onSuccess();

                return;
            }

            var uploadPromise = $scope.attachedFiles.length
                ? pictureRepository.upload($scope.attachedFiles)
                : Promise.resolve({
                      data: undefined,
                  });

            uploadPromise.then(function (result) {
                updateServiceRequestModel(result.data);

                if (isKudosCategorySelected()) {
                    $scope.serviceRequest.kudosShopItemId = $scope.serviceRequest.kudosShopItem.id;
                }

                var updateOrCreateRequest = isEditing()
                    ? serviceRequestRepository.put
                    : serviceRequestRepository.post;

                updateOrCreateRequest($scope.serviceRequest).then(onSuccess, onError);
            });
        };

        $scope.cancel = function () {
            $scope.newComment.content = '';

            $uibModalInstance.close();
            $scope.getServiceRequestsList();
        };

        $scope.updateSelectedShopItemInfo = function () {
            if (!!$scope.serviceRequest.kudosShopItem) {
                fillShopItemInfo();
            } else {
                resetShopItemInfo();
            }
        };

        $scope.resetShopItemFields = function () {
            if (!!$scope.serviceRequest.kudosShopItem) {
                resetShopItemInfo();
            }
        };

        function isServiceRequestDone() {
            return $scope.serviceRequest.status.title === 'Done';
        }

        function isServiceRequestModelUpdated() {
            return !angular.equals($scope.initialModel, $scope.serviceRequest);
        }

        function isKudosCategorySelected() {
            return $scope.serviceRequest.serviceRequestCategory.name === 'Kudos';
        }

        function updateServiceRequestModel(pictureId) {
            $scope.serviceRequest.pictureId = pictureId;
            $scope.serviceRequest.statusId = $scope.serviceRequest.status.id;
            $scope.serviceRequest.priorityId =
                $scope.serviceRequest.priority.id;
            $scope.serviceRequest.serviceRequestCategoryId =
                $scope.serviceRequest.serviceRequestCategory.id;
        }

        function fillShopItemInfo() {
            $scope.serviceRequest.title = $scope.serviceRequest.kudosShopItem.name;
            $scope.serviceRequest.kudosAmmount =
                $scope.serviceRequest.kudosShopItem.price;
            $scope.serviceRequest.description =
                $scope.serviceRequest.kudosShopItem.description;
            $scope.serviceRequest.pictureId =
                $scope.serviceRequest.kudosShopItem.pictureId;
        }

        function resetShopItemInfo() {
            $scope.serviceRequest.title = '';
            $scope.serviceRequest.kudosAmmount = '';
            $scope.serviceRequest.description = '';
            $scope.serviceRequest.pictureId = '';
            $scope.serviceRequest.kudosShopItem = null;
        }

        function onSuccess() {
            notifySrv.success('common.infoSaved');
            $scope.newComment.content = null;
            $uibModalInstance.close();
            $scope.getServiceRequestsList();
        }

        function onError(response) {
            $scope.isDisabled = false;
            notifySrv.error(response.data);
        }

        function isEditing() {
            return $scope.edit;
        }

        function isAdminOrAssignee() {
            return $scope.isAdmin || $scope.isAssignee;
        }

        function attachImage(input) {
            var options = {
                canvas: true,
            };

            if (input.value) {
                if (
                    shroomsFileUploader.validate(
                        input.files,
                        imageValidationSettings,
                        showUploadAlert
                    )
                ) {
                    $scope.attachedFiles = shroomsFileUploader.fileListToArray(
                        input.files
                    );
                    var displayImg = function (img) {
                        $scope.$apply(function ($scope) {
                            var fileName = $scope.attachedFiles[0].name;

                            $scope.serviceRequest.imageSource = img.toDataURL(
                                $scope.attachedFiles[0].type
                            );
                            if ($scope.attachedFiles[0].type !== 'image/gif') {
                                $scope.attachedFiles[0] =
                                    dataHandler.dataURItoBlob(
                                        $scope.serviceRequest.imageSource,
                                        $scope.attachedFiles[0].type
                                    );
                                $scope.attachedFiles[0].name = fileName;
                            }
                        });
                    };

                    loadImage.parseMetaData(
                        $scope.attachedFiles[0],
                        function (data) {
                            if (data.exif) {
                                options.orientation =
                                    data.exif.get('Orientation');
                            }

                            loadImage(
                                $scope.attachedFiles[0],
                                displayImg,
                                options
                            );
                        }
                    );
                }
            }
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
})();
