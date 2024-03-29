<template>
  <VcLayoutWithRightSidebar is-sidebar-sticky>
    <VcWidget id="review-order" :title="$t('common.titles.review_order')" prepend-icon="clipboard-copy-1" size="lg">
      <!-- Items grouped by Vendor -->
      <div v-if="$cfg.line_items_group_by_vendor_enabled" class="space-y-5 md:space-y-7">
        <template v-for="(group, vendorId) in selectedLineItemsGroupedByVendor" :key="vendorId">
          <div v-if="group.items.length" class="space-y-3">
            <!-- Vendor -->
            <div class="flex max-w-full flex-wrap gap-x-3">
              <VcVendor :vendor="group.vendor" />
              <VcRating v-if="$cfg.rating_enabled && group.vendor?.rating" :rating="group.vendor.rating" />
            </div>

            <OrderLineItems :items="group.items" />
          </div>
        </template>
      </div>

      <!-- Items not grouped by Vendor -->
      <OrderLineItems v-else :items="selectedLineItems" />

      <div class="divide-y print:divide-y-0 lg:divide-y-0">
        <!-- Shipping details -->
        <div v-if="!allItemsAreDigital" class="mt-6 flex flex-col gap-6 md:mt-8 lg:flex-row lg:gap-8">
          <div class="print:break-inside-avoid lg:w-3/5">
            <VcLabel>
              {{ $t("shared.checkout.shipping_details_section.labels.shipping_address") }}
            </VcLabel>

            <div class="grow divide-y rounded border print:border-none">
              <VcAddressSelection
                :address="shipment?.deliveryAddress"
                class="min-h-[4.625rem] px-3 py-1.5 print:min-h-0 print:px-0"
                readonly
              />
            </div>
          </div>

          <VcSelect
            :model-value="shippingMethodId"
            :label="$t('shared.checkout.shipping_details_section.labels.shipping_method')"
            :items="availableShippingMethods"
            value-field="id"
            size="auto"
            class="lg:w-2/5"
            readonly
          >
            <template #selected="{ item }">
              <VcSelectItem class="print:px-0 print:py-1.5">
                <VcSelectItemImage :src="item.logoUrl" class="print:hidden" />
                <VcSelectItemText>
                  {{ $t(`common.methods.delivery_by_id.${item.id}`) }}
                </VcSelectItemText>
              </VcSelectItem>
            </template>
          </VcSelect>
        </div>

        <!-- Payment details -->
        <div class="mb-2 mt-6 flex flex-col gap-6 pt-5 md:mt-8 lg:flex-row lg:gap-8 lg:pt-0">
          <div class="print:break-inside-avoid lg:w-3/5">
            <VcLabel>
              {{ $t("shared.checkout.billing_details_section.labels.billing_address") }}
            </VcLabel>

            <div class="grow divide-y rounded border print:border-none">
              <VcAddressSelection
                :address="billingAddress"
                class="min-h-[4.625rem] px-3 py-1.5 print:min-h-0 print:px-0"
                readonly
              />
            </div>
          </div>

          <div class="space-y-3 lg:w-2/5">
            <VcSelect
              :model-value="payment?.paymentGatewayCode"
              :label="$t('shared.checkout.billing_details_section.labels.payment_method')"
              :items="availablePaymentMethods"
              value-field="code"
              size="auto"
              readonly
            >
              <template #selected="{ item }">
                <VcSelectItem class="print:px-0 print:py-1.5">
                  <VcSelectItemImage :src="item.logoUrl" class="print:hidden" />
                  <VcSelectItemText>{{ $t(`common.methods.payment_by_code.${item.code}`) }}</VcSelectItemText>
                </VcSelectItem>
              </template>
            </VcSelect>

            <VcInput
              v-if="isPurchaseOrderNumberEnabled && purchaseOrderNumber"
              :model-value="purchaseOrderNumber"
              name="purchaseOrderNumber"
              readonly
            />
          </div>
        </div>
      </div>
    </VcWidget>

    <AcceptedGifts v-if="cart?.gifts?.length" :items="cart.gifts" />

    <OrderCommentSection v-if="comment" :comment="comment" readonly />

    <template #sidebar>
      <OrderSummary :cart="cart!" :selected-items="selectedLineItems" :no-shipping="allItemsAreDigital" footnote>
        <template #footer>
          <!-- Promotion code -->
          <transition name="slide-fade-top" mode="in-out" appear>
            <VcActionInput
              v-if="couponCode"
              :model-value="couponCode"
              :label="$t('common.labels.promotion_code')"
              class="mt-4"
              disabled
              readonly
            />
          </transition>

          <VcButton
            :disabled="isDisabledOrderCreation"
            :loading="loading"
            full-width
            class="mt-4 print:!hidden"
            @click="createOrderFromCart"
          >
            {{ $t("common.buttons.place_order") }}
          </VcButton>

          <transition name="slide-fade-top" mode="out-in" appear>
            <VcAlert v-show="hasValidationErrors" color="warning" size="sm" variant="solid-light" class="mt-4" icon>
              {{ $t("common.messages.something_went_wrong") }}
            </VcAlert>
          </transition>
        </template>
      </OrderSummary>

      <!-- Order actions -->
      <VcWidget :title="$t('common.titles.other_actions')" class="print:hidden">
        <VcButton full-width variant="outline" prepend-icon="printer" @click="print()">
          {{ $t("common.buttons.print_order") }}
        </VcButton>
      </VcWidget>
    </template>
  </VcLayoutWithRightSidebar>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { OrderLineItems } from "@/shared/account";
import { useCart, useCoupon } from "@/shared/cart";
import { AcceptedGifts, OrderCommentSection, OrderSummary, useCheckout } from "@/shared/checkout";
import type { CartAddressType } from "@/core/api/graphql/types";

const {
  cart,
  shipment,
  payment,
  selectedLineItems,
  selectedLineItemsGroupedByVendor,
  availableShippingMethods,
  availablePaymentMethods,
  hasValidationErrors,
  allItemsAreDigital,
} = useCart();
const {
  loading,
  billingAddressEqualsShipping,
  comment,
  purchaseOrderNumber,
  isPurchaseOrderNumberEnabled,
  isValidCheckout,
  createOrderFromCart,
} = useCheckout();
const { couponCode } = useCoupon();
const isDisabledOrderCreation = computed<boolean>(() => !isValidCheckout.value);

const shippingMethodId = computed(
  () => shipment.value?.shipmentMethodCode + "_" + shipment.value?.shipmentMethodOption,
);
const billingAddress = computed<CartAddressType | undefined>(() =>
  !allItemsAreDigital.value && billingAddressEqualsShipping.value
    ? shipment.value?.deliveryAddress
    : payment.value?.billingAddress,
);

function print() {
  window.print();
}
</script>

<style scoped lang="scss">
@media print {
  #review-order {
    :deep(.vc-section-widget__title) {
      @apply hidden;
    }
  }
}
</style>
