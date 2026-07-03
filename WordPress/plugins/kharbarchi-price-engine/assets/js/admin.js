jQuery(document).ready(function ($) {

    let baseImageFrame;
    let baseGalleryFrame;

    $(document).on('click', '.kharbarchi-select-base-image', function (e) {
        e.preventDefault();

        const button = $(this);
        const wrapper = button.closest('.kharbarchi-media-field');
        const input = wrapper.find('.kharbarchi-base-image-id');
        const preview = wrapper.find('.kharbarchi-base-image-preview');

        baseImageFrame = wp.media({
            title: 'انتخاب تصویر پایه کالا',
            button: {
                text: 'استفاده از این تصویر'
            },
            multiple: false
        });

        baseImageFrame.on('select', function () {
            const attachment = baseImageFrame
                .state()
                .get('selection')
                .first()
                .toJSON();

            input.val(attachment.id);

            const imageUrl =
                attachment.sizes && attachment.sizes.thumbnail
                    ? attachment.sizes.thumbnail.url
                    : attachment.url;

            preview.html(
                '<img src="' + imageUrl + '" style="max-width:100px;height:auto;">'
            );
        });

        baseImageFrame.open();
    });

    $(document).on('click', '.kharbarchi-remove-base-image', function (e) {
        e.preventDefault();

        const wrapper = $(this).closest('.kharbarchi-media-field');

        wrapper.find('.kharbarchi-base-image-id').val('');
        wrapper.find('.kharbarchi-base-image-preview').html('');
    });

    $(document).on('click', '.kharbarchi-select-base-gallery', function (e) {
        e.preventDefault();

        const button = $(this);
        const wrapper = button.closest('.kharbarchi-media-field');
        const input = wrapper.find('.kharbarchi-base-gallery-ids');
        const preview = wrapper.find('.kharbarchi-base-gallery-preview');

        baseGalleryFrame = wp.media({
            title: 'انتخاب گالری تصاویر پایه کالا',
            button: {
                text: 'استفاده از تصاویر انتخاب‌شده'
            },
            multiple: true
        });

        baseGalleryFrame.on('select', function () {
            const attachments = baseGalleryFrame
                .state()
                .get('selection')
                .toJSON();

            let ids = [];
            let html = '';

            attachments.forEach(function (attachment) {
                ids.push(attachment.id);

                const imageUrl =
                    attachment.sizes && attachment.sizes.thumbnail
                        ? attachment.sizes.thumbnail.url
                        : attachment.url;

                html += '<img src="' + imageUrl + '" style="max-width:80px;height:auto;margin:4px;">';
            });

            input.val(ids.join(','));
            preview.html(html);
        });

        baseGalleryFrame.open();
    });

    $(document).on('click', '.kharbarchi-remove-base-gallery', function (e) {
        e.preventDefault();

        const wrapper = $(this).closest('.kharbarchi-media-field');

        wrapper.find('.kharbarchi-base-gallery-ids').val('');
        wrapper.find('.kharbarchi-base-gallery-preview').html('');
    });

});